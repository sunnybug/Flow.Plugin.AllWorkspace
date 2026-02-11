using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flow.Launcher.Plugin.AllWorkspace.VSCode
{
    public class VSCodeInstance
    {
        public string ExecutablePath { get; set; } = string.Empty;
        public string AppData { get; set; } = string.Empty;
        public BitmapImage WorkspaceIconBitMap { get; set; }
        public BitmapImage RemoteIconBitMap { get; set; }

        public ImageSource WorkspaceIcon() => WorkspaceIconBitMap;
        public ImageSource RemoteIcon() => RemoteIconBitMap;
    }
}
