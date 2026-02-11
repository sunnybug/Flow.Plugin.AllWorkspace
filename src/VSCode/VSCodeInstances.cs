using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Flow.Launcher.Plugin.AllWorkspace.VSCode
{
    public static class VSCodeInstances
    {
        public static List<VSCodeInstance> Instances { get; set; } = new();

        private static bool IsVSCodeExecutable(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                var vi = FileVersionInfo.GetVersionInfo(path);
                var product = vi.ProductName ?? "";
                if (!product.Contains("Visual Studio Code", StringComparison.OrdinalIgnoreCase)) return false;
                if (!string.IsNullOrEmpty(vi.CompanyName) && !vi.CompanyName.Contains("Microsoft", StringComparison.OrdinalIgnoreCase)) return false;
                return true;
            }
            catch { return false; }
        }

        private static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = memory;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        private static Bitmap BitmapOverlayToCenter(Bitmap b1, Bitmap overlay)
        {
            int w = b1.Width, h = b1.Height;
            var resized = new Bitmap(overlay, new System.Drawing.Size(w / 2, h / 2));
            float left = (float)((w * 0.7) - (resized.Width * 0.5));
            float top = (float)((h * 0.7) - (resized.Height * 0.5));
            var final = new Bitmap(w, h);
            using (var g = Graphics.FromImage(final))
            {
                g.DrawImage(b1, System.Drawing.Point.Empty);
                g.DrawImage(resized, left, top);
            }
            return final;
        }

        public static void Load(Action<string>? log = null)
        {
            Instances = new List<VSCodeInstance>();
            var userAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            var paths = pathEnv.Split(';').Where(p =>
                p.Contains("VS Code", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("VisualStudioCode", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("vscode", StringComparison.OrdinalIgnoreCase) && !p.Contains("Cursor", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("codium", StringComparison.OrdinalIgnoreCase)).ToArray();

            foreach (var path in paths)
            {
                if (!Directory.Exists(path)) continue;
                var binPath = string.Equals(Path.GetFileName(path), "bin", StringComparison.OrdinalIgnoreCase) ? path : Path.Combine(path, "bin");
                var parentCodeExe = Path.Combine(Path.GetDirectoryName(binPath) ?? path, "Code.exe");
                string exe = null;
                string versionName = "Code";
                if (File.Exists(parentCodeExe) && IsVSCodeExecutable(parentCodeExe))
                {
                    exe = parentCodeExe;
                    versionName = "Code";
                }
                if (string.IsNullOrEmpty(exe))
                {
                    foreach (var name in new[] { "Code.exe", "Code - Insiders.exe", "code-insiders.exe", "VSCodium.exe", "codium.exe" })
                    {
                        var full = Path.Combine(path, name);
                        if (File.Exists(full) && IsVSCodeExecutable(full))
                        {
                            exe = full;
                            if (name.Contains("Insiders")) versionName = "Code - Insiders";
                            else if (name.Contains("codium")) versionName = "VSCodium";
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(exe) && Directory.Exists(binPath))
                {
                    exe = Directory.EnumerateFiles(binPath, "*.exe").FirstOrDefault(f => IsVSCodeExecutable(f));
                    if (exe != null) versionName = Path.GetFileNameWithoutExtension(exe);
                }
                if (string.IsNullOrEmpty(exe)) continue;

                var instance = new VSCodeInstance
                {
                    ExecutablePath = exe,
                    AppData = Directory.Exists(Path.Combine(Path.GetDirectoryName(exe) ?? "", "data")) ? Path.Combine(Path.GetDirectoryName(exe) ?? "", "data", "user-data") : Path.Combine(userAppData, versionName)
                };
                try
                {
                    var iconPath = Path.Combine(Path.GetDirectoryName(exe) ?? path, versionName + ".exe");
                    var bitmapIcon = Icon.ExtractAssociatedIcon(iconPath)?.ToBitmap();
                    var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                    var folderPath = Path.Combine(dir, "Images", "folder.png");
                    var monitorPath = Path.Combine(dir, "Images", "monitor.png");
                    var folder = File.Exists(folderPath) ? (Bitmap)Image.FromFile(folderPath) : new Bitmap(32, 32);
                    var monitor = File.Exists(monitorPath) ? (Bitmap)Image.FromFile(monitorPath) : new Bitmap(32, 32);
                    instance.WorkspaceIconBitMap = Bitmap2BitmapImage(BitmapOverlayToCenter(folder, bitmapIcon ?? folder));
                    instance.RemoteIconBitMap = Bitmap2BitmapImage(BitmapOverlayToCenter(monitor, bitmapIcon ?? monitor));
                }
                catch (Exception ex) { log?.Invoke($"[VSCode] 图标失败: {ex.Message}"); }
                Instances.Add(instance);
            }
            if (Instances.Count > 0)
                log?.Invoke($"[VSCode] 已加载 {Instances.Count} 个实例");
        }
    }
}
