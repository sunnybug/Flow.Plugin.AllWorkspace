using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.AllWorkspace.Shared
{
    public static class ParseVSCodeUri
    {
        private static readonly Regex LocalWorkspace = new Regex("^file:///(.+)$", RegexOptions.Compiled);
        private static readonly Regex RemoteSSHWorkspace = new Regex(@"^vscode-remote://ssh-remote\+(.+?(?=\/))(.+)$", RegexOptions.Compiled);
        private static readonly Regex RemoteWSLWorkspace = new Regex(@"^vscode-remote://wsl\+(.+?(?=\/))(.+)$", RegexOptions.Compiled);
        private static readonly Regex CodespacesWorkspace = new Regex(@"^vscode-remote://vsonline\+(.+?(?=\/))(.+)$", RegexOptions.Compiled);
        private static readonly Regex DevContainerWorkspace = new Regex(@"^vscode-remote://dev-container\+(.+?(?=\/))(.+)$", RegexOptions.Compiled);

        public static (WorkspaceKind? Kind, string? MachineName, string? Path) GetTypeWorkspace(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return (null, null, null);

            if (LocalWorkspace.IsMatch(uri))
            {
                var match = LocalWorkspace.Match(uri);
                if (match.Groups.Count > 1)
                    return (WorkspaceKind.Local, null, match.Groups[1].Value);
            }
            else if (RemoteSSHWorkspace.IsMatch(uri))
            {
                var match = RemoteSSHWorkspace.Match(uri);
                if (match.Groups.Count > 1)
                    return (WorkspaceKind.RemoteSSH, match.Groups[1].Value, match.Groups[2].Value);
            }
            else if (RemoteWSLWorkspace.IsMatch(uri))
            {
                var match = RemoteWSLWorkspace.Match(uri);
                if (match.Groups.Count > 1)
                    return (WorkspaceKind.RemoteWSL, match.Groups[1].Value, match.Groups[2].Value);
            }
            else if (CodespacesWorkspace.IsMatch(uri))
            {
                var match = CodespacesWorkspace.Match(uri);
                if (match.Groups.Count > 1)
                    return (WorkspaceKind.Codespaces, null, match.Groups[2].Value);
            }
            else if (DevContainerWorkspace.IsMatch(uri))
            {
                var match = DevContainerWorkspace.Match(uri);
                if (match.Groups.Count > 1)
                    return (WorkspaceKind.DevContainer, null, match.Groups[2].Value);
            }

            return (null, null, null);
        }
    }
}
