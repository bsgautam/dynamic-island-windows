using System;
using System.IO;
using System.Reflection;

namespace DynamicIsland.Services
{
    public static class StartupManager
    {
        private const string ShortcutName = "DynamicIsland.lnk";

        private static string GetShortcutPath()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            return Path.Combine(startupFolder, ShortcutName);
        }

        public static bool IsStartupEnabled()
        {
            return File.Exists(GetShortcutPath());
        }

        public static void SetStartup(bool enable)
        {
            try
            {
                string shortcutPath = GetShortcutPath();
                if (enable)
                {
                    string exePath = Assembly.GetEntryAssembly().Location;
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType != null)
                    {
                        dynamic shell = Activator.CreateInstance(shellType);
                        dynamic shortcut = shell.CreateShortcut(shortcutPath);
                        shortcut.TargetPath = exePath;
                        shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                        shortcut.Description = "iOS Dynamic Island for Windows";
                        shortcut.Save();
                    }
                }
                else
                {
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
