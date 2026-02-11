using Flow.Launcher.Plugin.AllWorkspace.Shared;

namespace Flow.Launcher.Plugin.AllWorkspace.VSCode
{
    public record VSCodeWorkspace
    {
        public PathString Path { get; init; }
        public PathString RelativePath { get; init; }
        public PathString FolderName { get; init; }
        public string Label { get; init; }
        public string ExtraInfo { get; init; }
        public WorkspaceKind WorkspaceLocation { get; init; }
        public bool IsWorkspaceFile { get; init; }
        public VSCodeInstance VSCodeInstance { get; init; }

        public string WorkspaceTypeToString()
        {
            return WorkspaceLocation switch
            {
                WorkspaceKind.Local => "本地",
                WorkspaceKind.Codespaces => "Codespaces",
                WorkspaceKind.RemoteContainers => "容器",
                WorkspaceKind.RemoteSSH => "SSH",
                WorkspaceKind.RemoteWSL => "WSL",
                WorkspaceKind.DevContainer => "Dev Container",
                _ => string.Empty
            };
        }
    }
}
