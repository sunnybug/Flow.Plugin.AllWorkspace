using Flow.Launcher.Plugin.AllWorkspace.Shared;

namespace Flow.Launcher.Plugin.AllWorkspace.Cursor
{
    public record CursorWorkspace
    {
        public PathString Path { get; init; }
        public PathString RelativePath { get; init; }
        public PathString FolderName { get; init; }
        public string Label { get; init; }
        public string ExtraInfo { get; init; }
        public WorkspaceKind TypeWorkspace { get; init; }
        public CursorInstance CursorInstance { get; init; }

        public string WorkspaceTypeToString()
        {
            return TypeWorkspace switch
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
