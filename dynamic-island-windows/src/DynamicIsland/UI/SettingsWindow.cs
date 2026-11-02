using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DynamicIsland.Config;
using DynamicIsland.Services;

namespace DynamicIsland.UI
{
    public class SettingsWindow : Window
    {
        private readonly IslandWindow _island;
        private readonly IslandSettings _settings;

        // Interactive Controls
        private Slider _topOffsetSlider;
        private TextBlock _topOffsetLabel;

        private Slider _idleWidthSlider;
        private TextBlock _idleWidthLabel;

        private Slider _idleHeightSlider;
        private TextBlock _idleHeightLabel;

        private Slider _expandedWidthSlider;
        private TextBlock _expandedWidthLabel;

        private Slider _expandedHeightSlider;
        private TextBlock _expandedHeightLabel;

        private CheckBox _expandOnHoverCheck;
        private CheckBox _borderGlowCheck;
        private CheckBox _startWithWindowsCheck;

        private CheckBox _enableMediaCheck;
        private CheckBox _enableBatteryCheck;
        private CheckBox _enableCapsLockCheck;
        private CheckBox _enableVolumeCheck;
        private CheckBox _enableStatsCheck;
        private CheckBox _enablePomodoroCheck;

        private TextBox _customTitleBox;
        private TextBox _customMsgBox;

        public SettingsWindow(IslandWindow island)
        {
            _island = island;
            _settings = island.Settings;

            Title = "Dynamic Island Control Center";
            Width = 580;
            Height = 720;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = ThemeColors.Brush(Color.FromRgb(18, 18, 20));
            Foreground = Brushes.White;
            FontFamily = new FontFamily("Segoe UI, Segoe UI Variable, sans-serif");

            BuildUi();
        }

        private void BuildUi()
        {
            var rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 1. Header Bar
            var headerBorder = new Border
            {
                Background = ThemeColors.Brush(Color.FromRgb(24, 24, 28)),
                BorderBrush = ThemeColors.Brush(Color.FromRgb(40, 40, 46)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(24, 18, 24, 18)
            };
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleStack = new StackPanel();
            var titleBlock = new TextBlock
            {
                Text = "🏝️ Dynamic Island Control Center",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            var subTitleBlock = new TextBlock
            {
                Text = "Configure, calibrate, and maintain your Windows Dynamic Island",
                FontSize = 12,
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                Margin = new Thickness(0, 3, 0, 0)
            };
            titleStack.Children.Add(titleBlock);
            titleStack.Children.Add(subTitleBlock);
            Grid.SetColumn(titleStack, 0);

            // Active Badge
            var badgeBorder = new Border
            {
                Background = ThemeColors.Brush(Color.FromArgb(40, 48, 209, 88)),
                BorderBrush = ThemeColors.Brush(ThemeColors.AccentGreen),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                VerticalAlignment = VerticalAlignment.Center
            };
            var badgeText = new TextBlock
            {
                Text = "● Island Active",
                Foreground = ThemeColors.Brush(ThemeColors.AccentGreen),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
            badgeBorder.Child = badgeText;
            Grid.SetColumn(badgeBorder, 1);

            headerGrid.Children.Add(titleStack);
            headerGrid.Children.Add(badgeBorder);
            headerBorder.Child = headerGrid;
            Grid.SetRow(headerBorder, 0);
            rootGrid.Children.Add(headerBorder);

            // 2. Scrollable Body
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(24, 16, 24, 16)
            };
            var bodyStack = new StackPanel();

            // Section: Position & Calibration
            bodyStack.Children.Add(CreateSectionHeader("📐 Position & Sizing (Live Calibration)"));
            var posCard = CreateCard();
            var posStack = new StackPanel();

            _topOffsetSlider = CreateSlider(0, 100, _settings.TopOffset, v =>
            {
                _settings.TopOffset = (int)v;
                _topOffsetLabel.Text = (int)v + " px";
                _island.ApplySettings(_settings);
            });
            _topOffsetLabel = new TextBlock { Text = _settings.TopOffset + " px", Foreground = ThemeColors.Brush(ThemeColors.AccentBlue), FontWeight = FontWeights.Bold };
            posStack.Children.Add(CreateSliderRow("Screen Top Offset (Webcam margin)", _topOffsetSlider, _topOffsetLabel));

            _idleWidthSlider = CreateSlider(120, 260, _settings.IdleWidth, v =>
            {
                _settings.IdleWidth = v;
                _idleWidthLabel.Text = (int)v + " px";
                _island.ApplySettings(_settings);
            });
            _idleWidthLabel = new TextBlock { Text = (int)_settings.IdleWidth + " px", Foreground = ThemeColors.Brush(ThemeColors.AccentBlue), FontWeight = FontWeights.Bold };
            posStack.Children.Add(CreateSliderRow("Idle Pill Width", _idleWidthSlider, _idleWidthLabel));

            _idleHeightSlider = CreateSlider(28, 46, _settings.IdleHeight, v =>
            {
                _settings.IdleHeight = v;
                _idleHeightLabel.Text = (int)v + " px";
                _island.ApplySettings(_settings);
            });
            _idleHeightLabel = new TextBlock { Text = (int)_settings.IdleHeight + " px", Foreground = ThemeColors.Brush(ThemeColors.AccentBlue), FontWeight = FontWeights.Bold };
            posStack.Children.Add(CreateSliderRow("Idle Pill Height", _idleHeightSlider, _idleHeightLabel));

            _expandedWidthSlider = CreateSlider(340, 520, _settings.ExpandedWidth, v =>
            {
                _settings.ExpandedWidth = v;
                _expandedWidthLabel.Text = (int)v + " px";
            });
            _expandedWidthLabel = new TextBlock { Text = (int)_settings.ExpandedWidth + " px", Foreground = ThemeColors.Brush(ThemeColors.AccentBlue), FontWeight = FontWeights.Bold };
            posStack.Children.Add(CreateSliderRow("Expanded Card Width", _expandedWidthSlider, _expandedWidthLabel));

            _expandedHeightSlider = CreateSlider(160, 260, _settings.ExpandedHeight, v =>
            {
                _settings.ExpandedHeight = v;
                _expandedHeightLabel.Text = (int)v + " px";
            });
            _expandedHeightLabel = new TextBlock { Text = (int)_settings.ExpandedHeight + " px", Foreground = ThemeColors.Brush(ThemeColors.AccentBlue), FontWeight = FontWeights.Bold };
            posStack.Children.Add(CreateSliderRow("Expanded Card Height", _expandedHeightSlider, _expandedHeightLabel));

            _expandOnHoverCheck = CreateCheckBox("Expand on Mouse Hover", _settings.ExpandOnHover);
            _borderGlowCheck = CreateCheckBox("Subtle Accent Halo Border", _settings.BorderGlow);
            _borderGlowCheck.Checked += (s, e) => { _settings.BorderGlow = true; _island.ApplySettings(_settings); };
            _borderGlowCheck.Unchecked += (s, e) => { _settings.BorderGlow = false; _island.ApplySettings(_settings); };

            posStack.Children.Add(_expandOnHoverCheck);
            posStack.Children.Add(_borderGlowCheck);

            posCard.Child = posStack;
            bodyStack.Children.Add(posCard);

            // Section: Accent Themes
            bodyStack.Children.Add(CreateSectionHeader("🎨 Accent Colors & Themes"));
            var themeCard = CreateCard();
            var themeStack = new StackPanel();
            var themesWrap = new WrapPanel { Margin = new Thickness(0, 4, 0, 4) };

            string[] themeNames = { "iOS Blue", "Emerald", "Cyan Glow", "Cyber Purple", "Sunset Gold", "Pure Silver" };
            string[] themeHexes = { "#0A84FF", "#30D158", "#64D2FF", "#BF5AF2", "#FF9F0A", "#E5E5EA" };

            for (int i = 0; i < themeNames.Length; i++)
            {
                string hex = themeHexes[i];
                string name = themeNames[i];
                var colorPill = CreateColorPresetButton(name, hex, () =>
                {
                    _settings.AccentColor = hex;
                    _island.ApplySettings(_settings);
                });
                themesWrap.Children.Add(colorPill);
            }
            themeStack.Children.Add(themesWrap);
            themeCard.Child = themeStack;
            bodyStack.Children.Add(themeCard);

            // Section: Modules & Features
            bodyStack.Children.Add(CreateSectionHeader("⚡ Dynamic Modules & Alerts"));
            var modCard = CreateCard();
            var modGrid = new Grid();
            modGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            modGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var modLeft = new StackPanel();
            _enableMediaCheck = CreateCheckBox("🎵 Media Player HUD", _settings.EnableMedia);
            _enableBatteryCheck = CreateCheckBox("🔋 Battery & Charger HUD", _settings.EnableBattery);
            _enableCapsLockCheck = CreateCheckBox("🔤 Caps Lock HUD Popups", _settings.EnableCapsLock);
            modLeft.Children.Add(_enableMediaCheck);
            modLeft.Children.Add(_enableBatteryCheck);
            modLeft.Children.Add(_enableCapsLockCheck);
            Grid.SetColumn(modLeft, 0);

            var modRight = new StackPanel();
            _enableVolumeCheck = CreateCheckBox("🔊 Volume Indicator HUD", _settings.EnableVolume);
            _enableStatsCheck = CreateCheckBox("📊 System Performance HUD", _settings.EnableSystemStats);
            _enablePomodoroCheck = CreateCheckBox("⏱️ Pomodoro / Stopwatch", _settings.EnablePomodoro);
            modRight.Children.Add(_enableVolumeCheck);
            modRight.Children.Add(_enableStatsCheck);
            modRight.Children.Add(_enablePomodoroCheck);
            Grid.SetColumn(modRight, 1);

            modGrid.Children.Add(modLeft);
            modGrid.Children.Add(modRight);
            modCard.Child = modGrid;
            bodyStack.Children.Add(modCard);

            // Section: Live Notification Simulator
            bodyStack.Children.Add(CreateSectionHeader("🧪 Live Notification Simulator (Instant Test)"));
            var simCard = CreateCard();
            var simStack = new StackPanel();

            var simButtonsWrap = new WrapPanel();
            simButtonsWrap.Children.Add(CreateTestButton("⚡ Power Plugged", () => _island.TriggerAlert("Power Connected", "89% • Fast Charging", Icons.Lightning, ThemeColors.AccentGreen)));
            simButtonsWrap.Children.Add(CreateTestButton("🔋 Low Battery", () => _island.TriggerAlert("Low Battery", "15% Remaining • Connect Charger", Icons.Lightning, ThemeColors.AccentOrange)));
            simButtonsWrap.Children.Add(CreateTestButton("🔤 Caps Lock ON", () => _island.TriggerAlert("Caps Lock", "CAPS LOCK ON", Icons.CapsLock, ThemeColors.AccentOrange)));
            simButtonsWrap.Children.Add(CreateTestButton("🔊 Volume 80%", () => _island.TriggerAlert("Volume", "80%", Icons.VolumeHigh, ThemeColors.AccentCyan)));
            simButtonsWrap.Children.Add(CreateTestButton("🎵 Song Change", () => _island.TriggerAlert("Now Playing", "Starboy - The Weeknd", Icons.Music, ThemeColors.AccentBlue)));
            simButtonsWrap.Children.Add(CreateTestButton("⏱️ Timer Done", () => _island.TriggerAlert("Pomodoro Timer", "Focus Session Complete! Great job.", Icons.Timer, ThemeColors.AccentGreen)));
            simStack.Children.Add(simButtonsWrap);

            // Custom Notification Inputs
            var customLabel = new TextBlock
            {
                Text = "Send Custom Notification Pill:",
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                FontSize = 11,
                Margin = new Thickness(0, 10, 0, 6)
            };
            simStack.Children.Add(customLabel);

            var customGrid = new Grid();
            customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _customTitleBox = new TextBox
            {
                Text = "Windows Notification",
                Background = ThemeColors.Brush(ThemeColors.CardColor),
                Foreground = Brushes.White,
                BorderBrush = ThemeColors.Brush(Color.FromRgb(50, 50, 56)),
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 6, 0)
            };
            _customMsgBox = new TextBox
            {
                Text = "iOS Dynamic Island running smoothly!",
                Background = ThemeColors.Brush(ThemeColors.CardColor),
                Foreground = Brushes.White,
                BorderBrush = ThemeColors.Brush(Color.FromRgb(50, 50, 56)),
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 6, 0)
            };
            var sendBtn = new Button
            {
                Content = "Send Pill",
                Background = ThemeColors.Brush(ThemeColors.AccentBlue),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 4, 12, 4),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            sendBtn.Click += (s, e) =>
            {
                _island.TriggerAlert(_customTitleBox.Text, _customMsgBox.Text, Icons.Sparkle, ThemeColors.ColorFromHex(_settings.AccentColor));
            };

            Grid.SetColumn(_customTitleBox, 0);
            Grid.SetColumn(_customMsgBox, 1);
            Grid.SetColumn(sendBtn, 2);
            customGrid.Children.Add(_customTitleBox);
            customGrid.Children.Add(_customMsgBox);
            customGrid.Children.Add(sendBtn);
            simStack.Children.Add(customGrid);

            simCard.Child = simStack;
            bodyStack.Children.Add(simCard);

            // Section: System & Startup
            bodyStack.Children.Add(CreateSectionHeader("🚀 System & Startup"));
            var sysCard = CreateCard();
            var sysStack = new StackPanel();

            _startWithWindowsCheck = CreateCheckBox("Launch Dynamic Island on Windows Startup", StartupManager.IsStartupEnabled());
            _startWithWindowsCheck.Checked += (s, e) => StartupManager.SetStartup(true);
            _startWithWindowsCheck.Unchecked += (s, e) => StartupManager.SetStartup(false);

            sysStack.Children.Add(_startWithWindowsCheck);
            sysCard.Child = sysStack;
            bodyStack.Children.Add(sysCard);

            scroll.Content = bodyStack;
            Grid.SetRow(scroll, 1);
            rootGrid.Children.Add(scroll);

            // 3. Footer Bar
            var footerBorder = new Border
            {
                Background = ThemeColors.Brush(Color.FromRgb(24, 24, 28)),
                BorderBrush = ThemeColors.Brush(Color.FromRgb(40, 40, 46)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(24, 14, 24, 14)
            };
            var footerGrid = new Grid();
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var resetBtn = new Button
            {
                Content = "Reset Defaults",
                Background = Brushes.Transparent,
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 6, 10, 6),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            resetBtn.Click += (s, e) => ResetDefaults();
            Grid.SetColumn(resetBtn, 0);

            var actionStack = new StackPanel { Orientation = Orientation.Horizontal };
            var expandToggleBtn = new Button
            {
                Content = "Toggle Expand",
                Background = ThemeColors.Brush(Color.FromRgb(40, 40, 46)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(14, 7, 14, 7),
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            expandToggleBtn.Click += (s, e) => _island.ExpandIsland();

            var saveBtn = new Button
            {
                Content = "💾 Save Settings",
                Background = ThemeColors.Brush(ThemeColors.AccentBlue),
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(18, 7, 18, 7),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            saveBtn.Click += (s, e) => SaveAndApply();

            actionStack.Children.Add(expandToggleBtn);
            actionStack.Children.Add(saveBtn);
            Grid.SetColumn(actionStack, 2);

            footerGrid.Children.Add(resetBtn);
            footerGrid.Children.Add(actionStack);
            footerBorder.Child = footerGrid;
            Grid.SetRow(footerBorder, 2);
            rootGrid.Children.Add(footerBorder);

            Content = rootGrid;
        }

        #region Helper Factories
        private TextBlock CreateSectionHeader(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                Margin = new Thickness(0, 16, 0, 8)
            };
        }

        private Border CreateCard()
        {
            return new Border
            {
                Background = ThemeColors.Brush(Color.FromRgb(28, 28, 32)),
                BorderBrush = ThemeColors.Brush(Color.FromRgb(45, 45, 52)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14)
            };
        }

        private Grid CreateSliderRow(string label, Slider slider, TextBlock valueDisplay)
        {
            var grid = new Grid { Margin = new Thickness(0, 4, 0, 6) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            var lbl = new TextBlock
            {
                Text = label,
                Foreground = Brushes.White,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            slider.VerticalAlignment = VerticalAlignment.Center;
            valueDisplay.VerticalAlignment = VerticalAlignment.Center;
            valueDisplay.HorizontalAlignment = HorizontalAlignment.Right;

            Grid.SetColumn(lbl, 0);
            Grid.SetColumn(slider, 1);
            Grid.SetColumn(valueDisplay, 2);

            grid.Children.Add(lbl);
            grid.Children.Add(slider);
            grid.Children.Add(valueDisplay);

            return grid;
        }

        private Slider CreateSlider(double min, double max, double val, Action<double> onChanged)
        {
            var slider = new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = val,
                IsSnapToTickEnabled = true,
                TickFrequency = 1,
                Margin = new Thickness(8, 0, 8, 0)
            };
            slider.ValueChanged += (s, e) => onChanged(e.NewValue);
            return slider;
        }

        private CheckBox CreateCheckBox(string label, bool isChecked)
        {
            var cb = new CheckBox
            {
                Content = label,
                IsChecked = isChecked,
                Foreground = Brushes.White,
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 5),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            return cb;
        }

        private Button CreateColorPresetButton(string label, string hex, Action onSelect)
        {
            var btn = new Button
            {
                Background = ThemeColors.Brush(Color.FromRgb(38, 38, 44)),
                BorderBrush = ThemeColors.Brush(Color.FromRgb(55, 55, 64)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 8, 8),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            var dot = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = ThemeColors.Brush(hex),
                Margin = new Thickness(0, 0, 6, 0)
            };
            var txt = new TextBlock
            {
                Text = label,
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.Medium
            };
            stack.Children.Add(dot);
            stack.Children.Add(txt);
            btn.Content = stack;
            btn.Click += (s, e) => onSelect();
            return btn;
        }

        private Button CreateTestButton(string label, Action onTest)
        {
            var btn = new Button
            {
                Content = label,
                Background = ThemeColors.Brush(Color.FromRgb(40, 40, 48)),
                Foreground = Brushes.White,
                BorderBrush = ThemeColors.Brush(Color.FromRgb(60, 60, 72)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 8, 8),
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btn.Click += (s, e) => onTest();
            return btn;
        }
        #endregion

        #region Actions
        private void SaveAndApply()
        {
            _settings.ExpandOnHover = _expandOnHoverCheck.IsChecked ?? true;
            _settings.EnableMedia = _enableMediaCheck.IsChecked ?? true;
            _settings.EnableBattery = _enableBatteryCheck.IsChecked ?? true;
            _settings.EnableCapsLock = _enableCapsLockCheck.IsChecked ?? true;
            _settings.EnableVolume = _enableVolumeCheck.IsChecked ?? true;
            _settings.EnableSystemStats = _enableStatsCheck.IsChecked ?? true;
            _settings.EnablePomodoro = _enablePomodoroCheck.IsChecked ?? true;

            _settings.Save();
            _island.ApplySettings(_settings);

            _island.TriggerAlert("Settings Saved", "Preferences updated successfully!", Icons.Check, ThemeColors.AccentGreen);
        }

        private void ResetDefaults()
        {
            var def = new IslandSettings();
            _settings.TopOffset = def.TopOffset;
            _settings.IdleWidth = def.IdleWidth;
            _settings.IdleHeight = def.IdleHeight;
            _settings.ExpandedWidth = def.ExpandedWidth;
            _settings.ExpandedHeight = def.ExpandedHeight;
            _settings.AccentColor = def.AccentColor;
            _settings.BorderGlow = def.BorderGlow;
            _settings.ExpandOnHover = def.ExpandOnHover;
            _settings.EnableMedia = def.EnableMedia;
            _settings.EnableBattery = def.EnableBattery;
            _settings.EnableCapsLock = def.EnableCapsLock;
            _settings.EnableVolume = def.EnableVolume;
            _settings.EnableSystemStats = def.EnableSystemStats;
            _settings.EnablePomodoro = def.EnablePomodoro;

            _topOffsetSlider.Value = def.TopOffset;
            _idleWidthSlider.Value = def.IdleWidth;
            _idleHeightSlider.Value = def.IdleHeight;
            _expandedWidthSlider.Value = def.ExpandedWidth;
            _expandedHeightSlider.Value = def.ExpandedHeight;
            _expandOnHoverCheck.IsChecked = def.ExpandOnHover;
            _borderGlowCheck.IsChecked = def.BorderGlow;

            _settings.Save();
            _island.ApplySettings(_settings);
        }
        #endregion
    }
}
