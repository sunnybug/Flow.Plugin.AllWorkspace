using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.AllWorkspace.Shared
{
    internal static class SystemPath
    {
        private static readonly Regex WindowsPath = new Regex(@"^([a-zA-Z]:)", RegexOptions.Compiled);

        public static string RealPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (WindowsPath.IsMatch(path))
            {
                var windowsPath = path.Replace("/", "\\");
                return char.ToUpperInvariant(windowsPath[0]) + windowsPath.Substring(1);
            }
            return path;
        }
    }
}
