using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.AllWorkspace.Cursor;
using Flow.Launcher.Plugin.AllWorkspace.Shared;
using Flow.Launcher.Plugin.AllWorkspace.Trae;
using Flow.Launcher.Plugin.AllWorkspace.VSCode;

namespace Flow.Launcher.Plugin.AllWorkspace
{
    /// <summary>Flow Launcher 插件：支持 Cursor / VSCode / Trae，快速打开 SSH 与最近打开目录。</summary>
    public class AllWorkspace : IPlugin, ISettingProvider, IContextMenu
    {
        private PluginInitContext _context;
        private Settings _settings;
        private readonly CursorWorkspacesApi _cursorWorkspaces = new();
        private readonly CursorRemoteMachinesApi _cursorMachines = new();
        private readonly TraeWorkspacesApi _traeWorkspaces = new();
        private readonly TraeRemoteMachinesApi _traeMachines = new();
        private readonly VSCodeWorkspacesApi _vscodeWorkspaces = new();
        private readonly VSCodeRemoteMachinesApi _vscodeMachines = new();

        /// <inheritdoc />
        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
            void Log(string msg) => _context.API.LogInfo("AllWorkspace", msg);

            CursorInstances.Load(Log);
            TraeInstances.Load(Log);
            VSCodeInstances.Load(Log);
        }

        /// <inheritdoc />
        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            var includeCursor = IncludeIde("cu", query.ActionKeyword);
            var includeTrae = IncludeIde("tr", query.ActionKeyword);
            var includeVscode = IncludeIde("vsc", query.ActionKeyword);

            if (_settings.DiscoverWorkspaces)
            {
                if (includeCursor) results.AddRange(GetCursorWorkspaceResults());
                if (includeTrae) results.AddRange(GetTraeWorkspaceResults());
                if (includeVscode) results.AddRange(GetVscodeWorkspaceResults());
                results.AddRange(GetCustomWorkspaceResults(includeCursor, includeTrae, includeVscode));
            }

            if (_settings.DiscoverMachines)
            {
                if (includeCursor) results.AddRange(GetCursorMachineResults());
                if (includeTrae) results.AddRange(GetTraeMachineResults());
                if (includeVscode) results.AddRange(GetVscodeMachineResults());
            }

            // 去重：同一 path/host 只保留一条（以首次出现的 IDE 为准）
            results = results
                .DistinctBy(r => (Title: r.Title, SubTitle: r.SubTitle))
                .ToList();

            if (query.ActionKeyword == string.Empty || (query.ActionKeyword != string.Empty && query.Search != string.Empty))
            {
                results = results.Where(r =>
                {
                    var matchResult = _context.API.FuzzySearch(query.Search, r.Title);
                    r.Score = matchResult.Score;
                    if (r.Score == 0 && !string.IsNullOrWhiteSpace(query.Search) && r.Title.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
                        r.Score = 1;
                    return r.Score > 0;
                }).ToList();
            }

            return results;
        }

        private bool IncludeIde(string keyword, string actionKeyword)
        {
            if (!_settings.DistinguishByIde) return true;
            return string.Equals(actionKeyword, keyword, StringComparison.OrdinalIgnoreCase);
        }

        private List<Result> GetCustomWorkspaceResults(bool includeCursor, bool includeTrae, bool includeVscode)
        {
            var list = new List<Result>();
            if (_settings.CustomWorkspaces == null || _settings.CustomWorkspaces.Count == 0) return list;

            string? exe = null;
            System.Func<System.Windows.Media.ImageSource>? icon = null;
            if (includeCursor && CursorInstances.Instances.Count > 0)
            {
                var inst = CursorInstances.Instances[0];
                exe = inst.ExecutablePath;
                icon = () => inst.WorkspaceIcon();
            }
            else if (includeTrae && TraeInstances.Instances.Count > 0)
            {
                var inst = TraeInstances.Instances[0];
                exe = inst.ExecutablePath;
                icon = () => inst.WorkspaceIcon;
            }
            else if (includeVscode && VSCodeInstances.Instances.Count > 0)
            {
                var inst = VSCodeInstances.Instances[0];
                exe = inst.ExecutablePath;
                icon = () => inst.WorkspaceIcon();
            }
            if (string.IsNullOrEmpty(exe) || icon == null) return list;

            foreach (var uri in _settings.CustomWorkspaces)
            {
                if (string.IsNullOrWhiteSpace(uri)) continue;
                var unescape = Uri.UnescapeDataString(uri);
                var (kind, _, path) = ParseVSCodeUri.GetTypeWorkspace(unescape);
                var title = !string.IsNullOrEmpty(path) ? Path.GetFileName(path.TrimEnd('/')) : unescape;
                if (string.IsNullOrEmpty(title)) title = unescape;

                list.Add(new Result
                {
                    Title = title,
                    SubTitle = "自定义工作区: " + (path ?? unescape),
                    Icon = () => icon(),
                    Action = c =>
                    {
                        try
                        {
                            if (c.SpecialKeyState.ToModifierKeys() == ModifierKeys.Control && kind == WorkspaceKind.Local && !string.IsNullOrEmpty(path))
                            {
                                _context.API.OpenDirectory(SystemPath.RealPath(path));
                                return true;
                            }
                            var process = new ProcessStartInfo
                            {
                                FileName = exe,
                                UseShellExecute = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            process.ArgumentList.Add("--folder-uri");
                            process.ArgumentList.Add(uri);
                            Process.Start(process);
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.ShowMsg(_context.CurrentPluginMetadata.Name, "无法启动", ex.Message);
                            return false;
                        }
                    },
                });
            }
            return list;
        }

        private List<Result> GetCursorWorkspaceResults()
        {
            var list = new List<Result>();
            var workspaces = _cursorWorkspaces.GetWorkspaces(s => _context.API.LogInfo("AllWorkspace", s));
            foreach (var ws in workspaces.Distinct())
            {
                var title = (string)ws.FolderName;
                if (string.IsNullOrEmpty(title)) title = "";
                var typeStr = ws.WorkspaceTypeToString();
                if (ws.TypeWorkspace != WorkspaceKind.Local)
                    title = !string.IsNullOrEmpty(ws.Label) ? ws.Label : $"{title}{(string.IsNullOrEmpty(ws.ExtraInfo) ? "" : " - " + ws.ExtraInfo)} ({typeStr})";
                var tooltip = $"工作区{(ws.TypeWorkspace != WorkspaceKind.Local ? " " + typeStr : "")}: {SystemPath.RealPath(ws.RelativePath)}";

                list.Add(new Result
                {
                    Title = title,
                    SubTitle = tooltip,
                    Icon = () => ws.CursorInstance.WorkspaceIcon(),
                    TitleToolTip = tooltip,
                    Action = c =>
                    {
                        try
                        {
                            if (c.SpecialKeyState.ToModifierKeys() == ModifierKeys.Control)
                            {
                                _context.API.OpenDirectory(SystemPath.RealPath(ws.RelativePath));
                                return true;
                            }
                            var process = new ProcessStartInfo
                            {
                                FileName = ws.CursorInstance.ExecutablePath,
                                UseShellExecute = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            process.ArgumentList.Add("--folder-uri");
                            process.ArgumentList.Add(ws.Path);
                            Process.Start(process);
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.ShowMsg(_context.CurrentPluginMetadata.Name, "无法启动 Cursor", ex.Message);
                            return false;
                        }
                    },
                    ContextData = ws,
                });
            }
            return list;
        }

        private List<Result> GetCursorMachineResults()
        {
            var list = new List<Result>();
            foreach (var a in _cursorMachines.GetMachines(s => _context.API.LogInfo("AllWorkspace", s)))
            {
                var title = $"SSH: {a.Host}";
                if (!string.IsNullOrEmpty(a.User) && !string.IsNullOrEmpty(a.HostName))
                    title += $" [{a.User}@{a.HostName}]";
                list.Add(new Result
                {
                    Title = title,
                    SubTitle = "远程 SSH 连接 (Cursor)",
                    Icon = () => a.CursorInstance.RemoteIcon(),
                    Action = c =>
                    {
                        try
                        {
                            var process = new ProcessStartInfo
                            {
                                FileName = a.CursorInstance.ExecutablePath,
                                UseShellExecute = true,
                                Arguments = $"--new-window --enable-proposed-api ms-vscode-remote.remote-ssh --remote ssh-remote+\"{a.Host}\"",
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            Process.Start(process);
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.ShowMsg(_context.CurrentPluginMetadata.Name, "无法启动 Cursor", ex.Message);
                            return false;
                        }
                    },
                    ContextData = a,
                });
            }
            return list;
        }

        private List<Result> GetTraeWorkspaceResults()
        {
            var list = new List<Result>();
            foreach (var ws in _traeWorkspaces.GetWorkspaces(s => _context.API.LogInfo("AllWorkspace", s)).Distinct())
            {
                var title = ws.FolderName ?? "";
                var typeStr = ws.WorkspaceTypeToString();
                if (ws.TypeWorkspace != TraeTypeWorkspace.Local)
                    title = !string.IsNullOrEmpty(ws.Label) ? ws.Label : $"{title}{(string.IsNullOrEmpty(ws.ExtraInfo) ? "" : " - " + ws.ExtraInfo)} ({typeStr})";
                var tooltip = $"工作区{(ws.TypeWorkspace != TraeTypeWorkspace.Local ? " " + typeStr : "")}: {ws.RelativePath}";

                list.Add(new Result
                {
                    Title = title,
                    SubTitle = tooltip,
                    Icon = () => ws.TraeInstance.WorkspaceIcon,
                    Action = c =>
                    {
                        try
                        {
                            if (c.SpecialKeyState.ToModifierKeys() == ModifierKeys.Control)
                            {
                                _context.API.OpenDirectory(ws.RelativePath);
                                return true;
                            }
                            var process = new ProcessStartInfo
                            {
                                FileName = ws.TraeInstance.ExecutablePath,
                                UseShellExecute = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            process.ArgumentList.Add("--folder-uri");
                            process.ArgumentList.Add(ws.Path);
                            Process.Start(process);
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.ShowMsg(_context.CurrentPluginMetadata.Name, "无法启动 Trae", ex.Message);
                            return false;
                        }
                    },
                    ContextData = ws,
                });
            }
            return list;
        }

        private List<Result> GetTraeMachineResults()
        {
            var list = new List<Result>();
            foreach (var a in _traeMachines.GetMachines(s => _context.API.LogInfo("AllWorkspace", s)))
            {
                var title = $"SSH: {a.Host}";
                if (!string.IsNullOrEmpty(a.User) && !string.IsNullOrEmpty(a.HostName))
                    title += $" [{a.User}@{a.HostName}]";
                list.Add(new Result
                {
                    Title = title,
                    SubTitle = "远程 SSH 连接 (Trae)",
                    Icon = () => a.TraeInstance.RemoteIcon,
                    Action = c =>
                    {
                        try
                        {
                            var process = new ProcessStartInfo
                            {
                                FileName = a.TraeInstance.ExecutablePath,
                                UseShellExecute = true,
                                Arguments = $"--new-window --enable-proposed-api ms-vscode-remote.remote-ssh --remote ssh-remote+\"{a.Host}\"",
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            Process.Start(process);
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.ShowMsg(_context.CurrentPluginMetadata.Name, "无法启动 Trae", ex.Message);
                            return false;
                        }
                    },
                    ContextData = a,
                });
            }
            return list;
        }

        private List<Result> GetVscodeWorkspaceResults()
        {
            var list = new List<Result>();
            foreach (var ws in _vscodeWorkspaces.GetWorkspaces(s => _context.API.LogInfo("AllWorkspace", s)).Distinct())
            {
                var title = (string)ws.FolderName;
                if (string.IsNullOrEmpty(title)) title = "";
                var typeStr = ws.WorkspaceTypeToString();
                if (ws.WorkspaceLocation != WorkspaceKind.Local)
                    title = !string.IsNullOrEmpty(ws.Label) ? ws.Label : $"{title}{(string.IsNullOrEmpty(ws.ExtraInfo) ? "" : " - " + ws.ExtraInfo)} ({typeStr})";
                var tooltip = $"工作区{(ws.WorkspaceLocation != WorkspaceKind.Local ? " " + typeStr : "")}: {SystemPath.RealPath(ws.RelativePath)}";

                list.Add(new Result
                {
                    Title = title,
                    SubTitle = tooltip,
                    Icon = () => ws.VSCodeInstance.WorkspaceIcon(),
                    TitleToolTip = tooltip,
                    Action = c =>
                    {
                        try
                        {
                            if (c.SpecialKeyState.ToModifierKeys() == ModifierKeys.Control)
                            {
                                _context.API.OpenDirectory(SystemPath.RealPath(ws.RelativePath));
                                return true;
                            }
                            var arg = ws.IsWorkspaceFile ? "--file-uri" : "--folder-uri";
                            var process = new ProcessStartInfo
                            {
                                FileName = ws.VSCodeInstance.ExecutablePath,
                                UseShellExecute = true,
                                Arguments = $"{arg} \"{ws.Path}\"",
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            Process.Start(process);
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.ShowMsg(_context.CurrentPluginMetadata.Name, "无法启动 VS Code", ex.Message);
                            return false;
                        }
                    },
                    ContextData = ws,
                });
            }
            return list;
        }

        private List<Result> GetVscodeMachineResults()
        {
            var list = new List<Result>();
            foreach (var a in _vscodeMachines.GetMachines(s => _context.API.LogInfo("AllWorkspace", s)))
            {
                var title = $"SSH: {a.Host}";
                if (!string.IsNullOrEmpty(a.User) && !string.IsNullOrEmpty(a.HostName))
                    title += $" [{a.User}@{a.HostName}]";
                list.Add(new Result
                {
                    Title = title,
                    SubTitle = "远程 SSH 连接 (VS Code)",
                    Icon = () => a.VSCodeInstance.RemoteIcon(),
                    Action = c =>
                    {
                        try
                        {
                            var process = new ProcessStartInfo
                            {
                                FileName = a.VSCodeInstance.ExecutablePath,
                                UseShellExecute = true,
                                Arguments = $"--new-window --enable-proposed-api ms-vscode-remote.remote-ssh --remote ssh-remote+\"{a.Host}\"",
                                WindowStyle = ProcessWindowStyle.Hidden,
                            };
                            Process.Start(process);
                            return true;
                        }
                        catch (Win32Exception ex)
                        {
                            _context.API.ShowMsg(_context.CurrentPluginMetadata.Name, "无法启动 VS Code", ex.Message);
                            return false;
                        }
                    },
                    ContextData = a,
                });
            }
            return list;
        }

        public Control CreateSettingPanel() => new SettingsView(_context, _settings);

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            var list = new List<Result>();

            if (selectedResult.ContextData is CursorWorkspace cws && cws.TypeWorkspace == WorkspaceKind.Local)
            {
                list.Add(new Result
                {
                    Title = "在文件资源管理器中打开",
                    SubTitle = SystemPath.RealPath(cws.RelativePath),
                    Icon = () => cws.CursorInstance.WorkspaceIcon(),
                    Action = _ => { _context.API.OpenDirectory(SystemPath.RealPath(cws.RelativePath)); return true; },
                });
            }
            else if (selectedResult.ContextData is TraeWorkspaceItem tws && tws.TypeWorkspace == TraeTypeWorkspace.Local)
            {
                list.Add(new Result
                {
                    Title = "在文件资源管理器中打开",
                    SubTitle = tws.RelativePath,
                    Icon = () => tws.TraeInstance.WorkspaceIcon,
                    Action = _ => { _context.API.OpenDirectory(tws.RelativePath); return true; },
                });
            }
            else if (selectedResult.ContextData is VSCodeWorkspace vws && vws.WorkspaceLocation == WorkspaceKind.Local)
            {
                list.Add(new Result
                {
                    Title = "在文件资源管理器中打开",
                    SubTitle = SystemPath.RealPath(vws.RelativePath),
                    Icon = () => vws.VSCodeInstance.WorkspaceIcon(),
                    Action = _ => { _context.API.OpenDirectory(SystemPath.RealPath(vws.RelativePath)); return true; },
                });
            }

            return list;
        }
    }
}
