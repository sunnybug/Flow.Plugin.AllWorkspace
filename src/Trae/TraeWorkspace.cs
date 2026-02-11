using Flow.Launcher.Plugin.AllWorkspace.Shared;

namespace Flow.Launcher.Plugin.AllWorkspace.Trae
{
    public enum TraeTypeWorkspace { Local = 1, Remote = 2, Container = 3 }

    public class TraeWorkspaceItem
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Label { get; set; }
        public string ExtraInfo { get; set; }
        public TraeTypeWorkspace TypeWorkspace { get; set; }
        public TraeInstance TraeInstance { get; set; }

        public string WorkspaceTypeToString()
        {
            return TypeWorkspace switch
            {
                TraeTypeWorkspace.Local => "本地",
                TraeTypeWorkspace.Remote => "远程",
                TraeTypeWorkspace.Container => "容器",
                _ => "Unknown"
            };
        }

        public override bool Equals(object? obj) => obj is TraeWorkspaceItem other && Path == other.Path;
        public override int GetHashCode() => Path?.GetHashCode() ?? 0;
    }
}
