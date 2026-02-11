using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Flow.Launcher.Plugin.AllWorkspace.Shared;
using Microsoft.Data.Sqlite;

namespace Flow.Launcher.Plugin.AllWorkspace.Cursor
{
    public class CursorWorkspacesApi
    {
        private static CursorWorkspace ParseUri(string uri, CursorInstance instance)
        {
            if (string.IsNullOrEmpty(uri)) return null;
            var unescapeUri = Uri.UnescapeDataString(uri);
            var (kind, machineName, path) = ParseVSCodeUri.GetTypeWorkspace(unescapeUri);
            if (!kind.HasValue) return null;

            var folderName = Path.GetFileName(unescapeUri);
            if (string.IsNullOrEmpty(folderName))
            {
                var dirInfo = new DirectoryInfo(unescapeUri);
                folderName = dirInfo.Name.TrimEnd(':');
            }

            return new CursorWorkspace
            {
                Path = unescapeUri,
                RelativePath = path ?? string.Empty,
                FolderName = folderName ?? string.Empty,
                ExtraInfo = machineName,
                TypeWorkspace = kind.Value,
                CursorInstance = instance
            };
        }

        private static readonly Regex WorkspaceLabelParser = new Regex("(.+?)(\\[.+\\])");

        public List<CursorWorkspace> GetWorkspaces(Action<string>? log = null)
        {
            var workspaces = new List<CursorWorkspace>();
            foreach (var instance in CursorInstances.Instances)
            {
                var storagePath = Path.Combine(instance.AppData, "storage.json");
                if (File.Exists(storagePath))
                {
                    try
                    {
                        var content = File.ReadAllText(storagePath);
                        var file = JsonSerializer.Deserialize<VSCodeStorageFile>(content);
                        if (file?.OpenedPathsList != null)
                        {
                            if (file.OpenedPathsList.Workspaces3 != null)
                                workspaces.AddRange(file.OpenedPathsList.Workspaces3
                                    .Select(uri => ParseUri(uri?.ToString(), instance))
                                    .Where(w => w != null)
                                    .Cast<CursorWorkspace>());
                            if (file.OpenedPathsList.Entries != null)
                                workspaces.AddRange(file.OpenedPathsList.Entries
                                    .Select(x => ParseUri(x.FolderUri, instance))
                                    .Where(w => w != null));
                        }
                    }
                    catch (Exception ex) { log?.Invoke($"[Cursor] storage.json 解析失败: {ex.Message}"); }
                }

                var stateDb = Path.Combine(instance.AppData, "User", "globalStorage", "state.vscdb");
                if (!File.Exists(stateDb)) continue;
                try
                {
                    using var conn = new SqliteConnection($"Data Source={stateDb};mode=readonly;cache=shared;");
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT value FROM ItemTable where key = 'history.recentlyOpenedPathsList'";
                    var result = cmd.ExecuteScalar();
                    if (result == null) continue;

                    using var doc = JsonDocument.Parse(result.ToString()!);
                    if (!doc.RootElement.TryGetProperty("entries", out var entries)) continue;
                    foreach (var entry in entries.EnumerateArray())
                    {
                        if (!entry.TryGetProperty("folderUri", out var folderUri)) continue;
                        var ws = ParseUri(folderUri.GetString(), instance);
                        if (ws == null) continue;
                        if (entry.TryGetProperty("label", out var label))
                        {
                            var labelStr = label.GetString() ?? "";
                            var m = WorkspaceLabelParser.Match(labelStr);
                            if (m.Success)
                                ws = ws with { Label = $"{m.Groups[2]} {m.Groups[1]}" };
                        }
                        workspaces.Add(ws);
                    }
                }
                catch (Exception ex) { log?.Invoke($"[Cursor] state.vscdb 读取失败: {ex.Message}"); }
            }
            return workspaces;
        }
    }
}
