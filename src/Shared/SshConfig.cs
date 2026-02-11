using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.AllWorkspace.Shared
{
    public static class SshConfig
    {
        private static readonly Regex SshConfigBlock = new Regex(@"^(\w[\s\S]*?\w)$(?=(?:\s+^\w|\z))", RegexOptions.Multiline);
        private static readonly Regex KeyValue = new Regex(@"(\w+\s\S+)", RegexOptions.Multiline);

        public static IEnumerable<SshHost> ParseFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return Array.Empty<SshHost>();
            return Parse(File.ReadAllText(path));
        }

        public static IEnumerable<SshHost> Parse(string str)
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<SshHost>();
            str = str.Replace('\r', '\0');
            var list = new List<SshHost>();
            foreach (Match match in SshConfigBlock.Matches(str))
            {
                var sshHost = new SshHost();
                var content = match.Groups.Values.First().Value;
                foreach (Match kv in KeyValue.Matches(content))
                {
                    var part = kv.Value;
                    var spaceIndex = part.IndexOf(' ');
                    if (spaceIndex <= 0) continue;
                    var key = part.Substring(0, spaceIndex);
                    var value = part.Substring(spaceIndex + 1).Trim();
                    if (!string.IsNullOrEmpty(key))
                        sshHost.Properties[key] = value;
                }
                list.Add(sshHost);
            }
            return list;
        }
    }

    public class SshHost
    {
        internal Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public string Host { get => this["Host"]?.ToString(); set => this["Host"] = value; }
        public string HostName { get => this["HostName"]?.ToString(); set => this["HostName"] = value; }
        public string User { get => this["User"]?.ToString(); set => this["User"] = value; }

        public object this[string key]
        {
            get => Properties.TryGetValue(key, out var v) ? v : null;
            set => Properties[key] = value;
        }
    }
}
