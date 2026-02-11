using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flow.Launcher.Plugin.AllWorkspace.Cursor
{
    public class CursorInstance
    {
        public string AppData { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = "cursor";
        public BitmapImage WorkspaceIconBitMap { get; set; }
        public BitmapImage RemoteIconBitMap { get; set; }

        public ImageSource WorkspaceIcon() => WorkspaceIconBitMap;
        public ImageSource RemoteIcon() => RemoteIconBitMap;
    }
}
