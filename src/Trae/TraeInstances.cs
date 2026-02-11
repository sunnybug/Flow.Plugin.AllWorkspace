using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Flow.Launcher.Plugin.AllWorkspace.Trae
{
    public static class TraeInstances
    {
        public static List<TraeInstance> Instances { get; set; } = new();

        private static string FindTraeExecutable(Action<string> log)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = "trae",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });
                if (process == null) return null;
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0) return null;
                var line = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return line?.Trim();
            }
            catch (Exception ex) { log?.Invoke($"[Trae] where.exe 失败: {ex.Message}"); return null; }
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
            Instances = new List<TraeInstance>();
            var userAppData = Environment.GetEnvironmentVariable("AppData") ?? "";
            var traeExe = FindTraeExecutable(log);
            if (!string.IsNullOrEmpty(traeExe) && File.Exists(traeExe))
            {
                var isCn = traeExe.Contains("Trae.CN", StringComparison.OrdinalIgnoreCase) || traeExe.Contains("Trae CN", StringComparison.OrdinalIgnoreCase);
                var appDataName = isCn ? "Trae CN" : "Trae";
                var instance = new TraeInstance
                {
                    ExecutablePath = traeExe,
                    AppData = Path.Combine(userAppData, appDataName)
                };
                try
                {
                    var icon = Icon.ExtractAssociatedIcon(traeExe)?.ToBitmap();
                    if (icon != null)
                    {
                        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                        var folderPath = Path.Combine(dir, "Images", "folder.png");
                        var monitorPath = Path.Combine(dir, "Images", "monitor.png");
                        var folder = File.Exists(folderPath) ? (Bitmap)Image.FromFile(folderPath) : new Bitmap(32, 32);
                        var monitor = File.Exists(monitorPath) ? (Bitmap)Image.FromFile(monitorPath) : new Bitmap(32, 32);
                        instance.WorkspaceIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(folder, icon));
                        instance.RemoteIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(monitor, icon));
                    }
                }
                catch (Exception ex) { log?.Invoke($"[Trae] 图标失败: {ex.Message}"); }
                Instances.Add(instance);
                log?.Invoke($"[Trae] 已加载 1 个实例");
                return;
            }

            var localAppData = Environment.GetEnvironmentVariable("LocalAppData") ?? "";
            var paths = new[]
            {
                Path.Combine(localAppData, "Programs", "trae"),
                Path.Combine(localAppData, "Programs", "Trae"),
                Path.Combine(localAppData, "Programs", "Trae CN"),
            };
            foreach (var path in paths)
            {
                if (!Directory.Exists(path)) continue;
                var files = Directory.EnumerateFiles(path).Where(f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToArray();
                if (files.Length == 0) continue;
                var exe = files[0];
                var isCn = path.Contains("Trae CN", StringComparison.OrdinalIgnoreCase);
                var instance = new TraeInstance
                {
                    ExecutablePath = exe,
                    AppData = Path.Combine(userAppData, isCn ? "Trae CN" : "Trae")
                };
                try
                {
                    var icon = Icon.ExtractAssociatedIcon(exe)?.ToBitmap();
                    if (icon != null)
                    {
                        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                        var folderPath = Path.Combine(dir, "Images", "folder.png");
                        var monitorPath = Path.Combine(dir, "Images", "monitor.png");
                        var folder = File.Exists(folderPath) ? (Bitmap)Image.FromFile(folderPath) : new Bitmap(32, 32);
                        var monitor = File.Exists(monitorPath) ? (Bitmap)Image.FromFile(monitorPath) : new Bitmap(32, 32);
                        instance.WorkspaceIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(folder, icon));
                        instance.RemoteIcon = Bitmap2BitmapImage(BitmapOverlayToCenter(monitor, icon));
                    }
                }
                catch { }
                Instances.Add(instance);
            }
            if (Instances.Count > 0)
                log?.Invoke($"[Trae] 已加载 {Instances.Count} 个实例");
        }
    }
}
