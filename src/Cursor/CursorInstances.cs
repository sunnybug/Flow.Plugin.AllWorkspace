using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Flow.Launcher.Plugin.AllWorkspace.Cursor
{
    public static class CursorInstances
    {
        public static List<CursorInstance> Instances { get; set; } = new();

        private static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private static Bitmap BitmapOverlayToCenter(Bitmap bitmap1, Bitmap overlayBitmap)
        {
            int w = bitmap1.Width, h = bitmap1.Height;
            var overlayResized = new Bitmap(overlayBitmap, new System.Drawing.Size(w / 2, h / 2));
            float left = (float)((w * 0.7) - (overlayResized.Width * 0.5));
            float top = (float)((h * 0.7) - (overlayResized.Height * 0.5));
            var final = new Bitmap(w, h);
            using (var g = Graphics.FromImage(final))
            {
                g.DrawImage(bitmap1, System.Drawing.Point.Empty);
                g.DrawImage(overlayResized, left, top);
            }
            return final;
        }

        public static void Load(Action<string>? log = null)
        {
            Instances = new List<CursorInstance>();
            var userAppData = Environment.GetEnvironmentVariable("AppData");
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var cursorAppData = Path.Combine(localAppData, "Programs", "cursor");
            var cursorExePath = Path.Combine(cursorAppData, "Cursor.exe");

            if (!File.Exists(cursorExePath))
            {
                log?.Invoke($"[Cursor] 未找到 Cursor: {cursorExePath}");
                return;
            }

            var instance = new CursorInstance
            {
                AppData = Path.Combine(userAppData ?? "", "Cursor"),
                ExecutablePath = cursorExePath
            };

            try
            {
                var cursorBitmap = Icon.ExtractAssociatedIcon(cursorExePath)?.ToBitmap();
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                var folderPath = Path.Combine(dir, "Images", "folder.png");
                var monitorPath = Path.Combine(dir, "Images", "monitor.png");
                var folderIcon = File.Exists(folderPath) ? (Bitmap)Image.FromFile(folderPath) : new Bitmap(32, 32);
                var monitorIcon = File.Exists(monitorPath) ? (Bitmap)Image.FromFile(monitorPath) : new Bitmap(32, 32);
                var iconForOverlay = cursorBitmap ?? folderIcon;
                instance.WorkspaceIconBitMap = Bitmap2BitmapImage(BitmapOverlayToCenter(folderIcon, iconForOverlay));
                instance.RemoteIconBitMap = Bitmap2BitmapImage(BitmapOverlayToCenter(monitorIcon, iconForOverlay));
            }
            catch (Exception ex)
            {
                log?.Invoke($"[Cursor] 图标加载失败: {ex.Message}");
            }

            Instances.Add(instance);
            log?.Invoke($"[Cursor] 已加载 1 个实例, AppData={instance.AppData}");
        }
    }
}
