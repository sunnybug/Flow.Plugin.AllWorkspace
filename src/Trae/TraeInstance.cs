using System;
using System.Windows.Media;

namespace Flow.Launcher.Plugin.AllWorkspace.Trae
{
    public class TraeInstance
    {
        public string AppData { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public ImageSource WorkspaceIcon { get; set; }
        public ImageSource RemoteIcon { get; set; }
    }
}
