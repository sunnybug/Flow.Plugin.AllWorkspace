using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Flow.Launcher.Plugin.AllWorkspace.Shared;

namespace Flow.Launcher.Plugin.AllWorkspace.VSCode
{
    public class VSCodeRemoteMachine
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string HostName { get; set; }
        public VSCodeInstance VSCodeInstance { get; set; }
    }

    public class VSCodeRemoteMachinesApi
    {
        private static string ExpandPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            path = path.Trim();
            if (path.StartsWith("~/", StringComparison.Ordinal) || path == "~")
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path.Length > 1 ? path.Substring(2) : "");
            else if (path.StartsWith("~\\", StringComparison.Ordinal))
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path.Substring(2));
            return Environment.ExpandEnvironmentVariables(path);
        }

        private static string DefaultSshConfigPath()
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");

        public List<VSCodeRemoteMachine> GetMachines(Action<string>? log = null)
        {
            var results = new List<VSCodeRemoteMachine>();
            foreach (var instance in VSCodeInstances.Instances)
            {
                var settingsPath = Path.Combine(instance.AppData, "User", "settings.json");
                string? configPath = null;
                if (File.Exists(settingsPath))
                {
                    try
                    {
                        var content = File.ReadAllText(settingsPath);
                        var el = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip });
                        if (el.TryGetProperty("remote.SSH.configFile", out var pathEl))
                        {
                            var raw = pathEl.GetString();
                            if (!string.IsNullOrWhiteSpace(raw)) configPath = ExpandPath(raw);
                        }
                    }
                    catch { }
                }
                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                    configPath = DefaultSshConfigPath();
                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath)) continue;

                try
                {
                    foreach (var h in SshConfig.ParseFile(configPath))
                    {
                        if (string.IsNullOrEmpty(h.Host)) continue;
                        results.Add(new VSCodeRemoteMachine
                        {
                            Host = h.Host,
                            HostName = h.HostName ?? "",
                            User = h.User ?? "",
                            VSCodeInstance = instance
                        });
                    }
                }
                catch (Exception ex) { log?.Invoke($"[VSCode] SSH config 失败: {ex.Message}"); }
            }
            return results;
        }
    }
}
