using System;
using System.IO;
using System.Web.Script.Serialization;

namespace DynamicIsland.Config
{
    public class IslandSettings
    {
        public int TopOffset { get; set; }
        public double IdleWidth { get; set; }
        public double IdleHeight { get; set; }
        public double ExpandedWidth { get; set; }
        public double ExpandedHeight { get; set; }
        public string AccentColor { get; set; }
        public bool BorderGlow { get; set; }
        public bool ExpandOnHover { get; set; }
        public double AutoCollapseSeconds { get; set; }
        public bool EnableMedia { get; set; }
        public bool EnableBattery { get; set; }
        public bool EnableCapsLock { get; set; }
        public bool EnableVolume { get; set; }
        public bool EnableSystemStats { get; set; }
        public bool EnablePomodoro { get; set; }
        public bool StartWithWindows { get; set; }
        public string CurrentTheme { get; set; }

        public IslandSettings()
        {
            TopOffset = 8;
            IdleWidth = 175.0;
            IdleHeight = 34.0;
            ExpandedWidth = 420.0;
            ExpandedHeight = 190.0;
            AccentColor = "#0A84FF";
            BorderGlow = true;
            ExpandOnHover = true;
            AutoCollapseSeconds = 3.5;
            EnableMedia = true;
            EnableBattery = true;
            EnableCapsLock = true;
            EnableVolume = true;
            EnableSystemStats = true;
            EnablePomodoro = true;
            StartWithWindows = false;
            CurrentTheme = "OLED Black";
        }

        private static string GetConfigPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "DynamicIslandWindows");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "settings.json");
        }

        public static IslandSettings Load()
        {
            try
            {
                string path = GetConfigPath();
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var serializer = new JavaScriptSerializer();
                    var settings = serializer.Deserialize<IslandSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch (Exception)
            {
                // Fallback to default on any parse error
            }
            return new IslandSettings();
        }

        public void Save()
        {
            try
            {
                string path = GetConfigPath();
                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(this);
                File.WriteAllText(path, json);
            }
            catch (Exception)
            {
            }
        }
    }
}
