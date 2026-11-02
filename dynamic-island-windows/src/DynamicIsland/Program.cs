using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using DynamicIsland.Config;
using DynamicIsland.UI;
using Application = System.Windows.Application;

namespace DynamicIsland
{
    public class Program
    {
        private static Mutex _appMutex;
        private static NotifyIcon _trayIcon;
        private static IslandWindow _island;
        private static SettingsWindow _settingsWindow;
        private static IslandSettings _settings;

        [STAThread]
        public static void Main(string[] args)
        {
            bool createdNew;
            _appMutex = new Mutex(true, "Local\\DynamicIsland_Windows_App_Mutex", out createdNew);

            if (!createdNew)
            {
                // Already running
                System.Windows.MessageBox.Show(
                    "Dynamic Island is already running in your system tray.\nLook near the bottom-right clock or click its tray icon to open the Control Center.",
                    "Dynamic Island for Windows",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Load settings
            _settings = IslandSettings.Load();

            // Create Floating Island
            _island = new IslandWindow(_settings);
            _island.OpenSettingsRequested += ShowSettings;
            _island.Show();

            // Setup System Tray Icon
            SetupTrayIcon();

            // If launched with --settings or -s, open settings window immediately
            bool openSettingsNow = false;
            foreach (var arg in args)
            {
                if (arg.Equals("--settings", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-s", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("/settings", StringComparison.OrdinalIgnoreCase))
                {
                    openSettingsNow = true;
                    break;
                }
            }

            if (openSettingsNow)
            {
                ShowSettings();
            }

            app.Run();

            // Cleanup
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
        }

        private static void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Text = "Dynamic Island for Windows",
                Icon = GeneratePillIcon(),
                Visible = true
            };

            // Double click opens Settings Window
            _trayIcon.DoubleClick += (s, e) => ShowSettings();

            // Context Menu
            var contextMenu = new ContextMenuStrip();

            var settingsItem = new ToolStripMenuItem("⚙️ Dynamic Island Control Center");
            settingsItem.Font = new Font(settingsItem.Font, System.Drawing.FontStyle.Bold);
            settingsItem.Click += (s, e) => ShowSettings();

            var testItem = new ToolStripMenuItem("✨ Test Notification Alert");
            testItem.Click += (s, e) =>
            {
                if (_island != null)
                {
                    _island.TriggerAlert("Dynamic Island", "Notification alert test active!", Icons.Sparkle, ThemeColors.AccentBlue);
                }
            };

            var toggleVisibilityItem = new ToolStripMenuItem("👁️ Toggle Island Visibility");
            toggleVisibilityItem.Click += (s, e) =>
            {
                if (_island != null)
                {
                    if (_island.Visibility == Visibility.Visible)
                        _island.Hide();
                    else
                        _island.Show();
                }
            };

            var restartItem = new ToolStripMenuItem("🔄 Restart Island");
            restartItem.Click += (s, e) =>
            {
                if (_island != null)
                {
                    _settings = IslandSettings.Load();
                    _island.ApplySettings(_settings);
                    _island.TriggerAlert("Dynamic Island", "Restarted successfully!", Icons.Check, ThemeColors.AccentGreen);
                }
            };

            var exitItem = new ToolStripMenuItem("❌ Exit");
            exitItem.Click += (s, e) =>
            {
                _trayIcon.Visible = false;
                Application.Current.Shutdown();
            };

            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(testItem);
            contextMenu.Items.Add(toggleVisibilityItem);
            contextMenu.Items.Add(restartItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = contextMenu;
        }

        private static void ShowSettings()
        {
            if (_settingsWindow == null || !_settingsWindow.IsLoaded)
            {
                _settingsWindow = new SettingsWindow(_island);
                _settingsWindow.Closed += (s, e) => _settingsWindow = null;
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
                if (_settingsWindow.WindowState == WindowState.Minimized)
                {
                    _settingsWindow.WindowState = WindowState.Normal;
                }
            }
        }

        private static Icon GeneratePillIcon()
        {
            // Dynamically draw a crisp 32x32 black pill icon with blue indicator dot
            using (var bmp = new Bitmap(32, 32))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(System.Drawing.Color.Transparent);

                // Draw black pill
                using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(16, 16, 20)))
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(10, 132, 255), 1.5f))
                {
                    var rect = new RectangleF(2, 8, 28, 16);
                    var path = new GraphicsPath();
                    path.AddArc(rect.X, rect.Y, rect.Height, rect.Height, 90, 180);
                    path.AddArc(rect.Right - rect.Height, rect.Y, rect.Height, rect.Height, 270, 180);
                    path.CloseFigure();

                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

                // Draw tiny blue status dot inside pill
                using (var dotBrush = new SolidBrush(System.Drawing.Color.FromArgb(10, 132, 255)))
                {
                    g.FillEllipse(dotBrush, 13, 13, 6, 6);
                }

                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }
    }
}
