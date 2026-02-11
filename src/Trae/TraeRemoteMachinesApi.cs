using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.AllWorkspace.Shared;

namespace Flow.Launcher.Plugin.AllWorkspace.Trae
{
    public class TraeRemoteMachine
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string HostName { get; set; }
        public TraeInstance TraeInstance { get; set; }
    }

    public class TraeRemoteMachinesApi
    {
        private static readonly string[] GitPlatforms = { "github.com", "gitee.com", "gitlab.com", "bitbucket.org", "coding.net", "code.aliyun.com", "dev.azure.com", "ssh.dev.azure.com", "sourceforge.net", "gitcode.com" };

        public List<TraeRemoteMachine> GetMachines(Action<string>? log = null)
        {
            var list = new List<TraeRemoteMachine>();
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");
            if (!File.Exists(configPath)) return list;

            try
            {
                foreach (var instance in TraeInstances.Instances)
                {
                    foreach (var h in SshConfig.ParseFile(configPath))
                    {
                        if (string.IsNullOrEmpty(h.Host) || h.Host.Contains("*") || h.Host.Contains("?")) continue;
                        if (!string.IsNullOrEmpty(h.HostName) && !string.IsNullOrEmpty(h.User) && string.Equals(h.User, "git", StringComparison.OrdinalIgnoreCase))
                        {
                            var hostLower = h.HostName.ToLowerInvariant();
                            if (GitPlatforms.Any(p => hostLower.EndsWith(p))) continue;
                        }
                        list.Add(new TraeRemoteMachine { Host = h.Host, User = h.User ?? "", HostName = h.HostName ?? "", TraeInstance = instance });
                    }
                }
            }
            catch (Exception ex) { log?.Invoke($"[Trae] SSH 解析失败: {ex.Message}"); }
            return list.Distinct().ToList();
        }
    }
}
