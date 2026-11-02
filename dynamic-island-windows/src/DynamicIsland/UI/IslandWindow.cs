using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using DynamicIsland.Config;
using DynamicIsland.Services;

namespace DynamicIsland.UI
{
    public enum IslandState
    {
        Idle,
        Alert,
        Expanded
    }

    public class IslandWindow : Window
    {
        #region Win32 Interop
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion

        // Dependencies & Settings
        public IslandSettings Settings { get; private set; }
        private readonly SystemMonitor _sysMonitor;
        private readonly BatteryMonitor _batteryMonitor;
        private readonly MediaMonitor _mediaMonitor;
        private readonly KeyboardHook _keyboardHook;

        // Timers
        private DispatcherTimer _pollTimer;
        private DispatcherTimer _collapseTimer;
        private DispatcherTimer _equalizerTimer;
        private DispatcherTimer _pomodoroTimer;

        // Current UI State
        private IslandState _currentState = IslandState.Idle;
        private int _activeTabIndex = 0; // 0=Media, 1=Stats, 2=Timer, 3=Tools

        // Pomodoro State
        private int _pomodoroRemainingSeconds = 25 * 60;
        private bool _pomodoroRunning = false;

        // UI Visual Elements
        private Border _mainBorder;
        private Grid _rootGrid;

        // Idle UI
        private Grid _idleGrid;
        private StackPanel _idleLeftPanel;
        private TextBlock _idleCenterText;
        private StackPanel _idleRightPanel;
        private Rectangle[] _miniEqualizerBars;

        // Alert UI
        private Grid _alertGrid;
        private Path _alertIcon;
        private TextBlock _alertTitle;
        private TextBlock _alertSub;

        // Expanded UI
        private Grid _expandedGrid;
        private Grid _tabsContentGrid;
        private Button[] _tabButtons;
        private Grid _mediaTabGrid;
        private Grid _statsTabGrid;
        private Grid _timerTabGrid;
        private Grid _toolsTabGrid;

        // Media UI elements
        private TextBlock _mediaTitleText;
        private TextBlock _mediaArtistText;
        private Path _mediaPlayPauseIcon;
        private Rectangle[] _largeEqualizerBars;
        private Slider _volumeSlider;

        // Stats UI elements
        private ProgressBar _cpuBar;
        private TextBlock _cpuText;
        private ProgressBar _ramBar;
        private TextBlock _ramText;
        private TextBlock _batteryStatsText;
        private TextBlock _uptimeText;

        // Timer UI elements
        private TextBlock _timerDisplay;
        private Button _timerStartBtn;

        public Action OpenSettingsRequested;

        public IslandWindow(IslandSettings settings)
        {
            Settings = settings;

            _sysMonitor = new SystemMonitor();
            _batteryMonitor = new BatteryMonitor();
            _mediaMonitor = new MediaMonitor();
            _keyboardHook = new KeyboardHook();

            InitializeWindowProperties();
            BuildUi();
            HookEvents();
            StartTimers();
        }

        private void InitializeWindowProperties()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.Manual;

            Width = Settings.IdleWidth;
            Height = Settings.IdleHeight;
            Top = Settings.TopOffset;
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2.0;

            SourceInitialized += (s, e) =>
            {
                var handle = new WindowInteropHelper(this).Handle;
                int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
                SetWindowLong(handle, GWL_EXSTYLE, exStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
            };
        }

        private void BuildUi()
        {
            _rootGrid = new Grid();

            // Main Pill Border
            _mainBorder = new Border
            {
                Background = new SolidColorBrush(ThemeColors.BackgroundColor),
                BorderBrush = Settings.BorderGlow ? ThemeColors.Brush(Settings.AccentColor) : ThemeColors.Brush(ThemeColors.BorderDefault),
                BorderThickness = new Thickness(Settings.BorderGlow ? 1.4 : 1.0),
                CornerRadius = new CornerRadius(Settings.IdleHeight / 2.0),
                ClipToBounds = true,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 18,
                    ShadowDepth = 3,
                    Opacity = 0.55
                }
            };

            // Build Sub-Views
            BuildIdleView();
            BuildAlertView();
            BuildExpandedView();

            _rootGrid.Children.Add(_idleGrid);
            _rootGrid.Children.Add(_alertGrid);
            _rootGrid.Children.Add(_expandedGrid);

            _mainBorder.Child = _rootGrid;
            Content = _mainBorder;

            SetViewState(IslandState.Idle);
        }

        #region Idle View
        private void BuildIdleView()
        {
            _idleGrid = new Grid
            {
                Margin = new Thickness(12, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var col1 = new ColumnDefinition { Width = GridLength.Auto };
            var col2 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col3 = new ColumnDefinition { Width = GridLength.Auto };
            _idleGrid.ColumnDefinitions.Add(col1);
            _idleGrid.ColumnDefinitions.Add(col2);
            _idleGrid.ColumnDefinitions.Add(col3);

            // Left: Mini Visualizer or Battery Icon
            _idleLeftPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            _miniEqualizerBars = new Rectangle[4];
            var barBrush = ThemeColors.Brush(Settings.AccentColor);
            for (int i = 0; i < 4; i++)
            {
                _miniEqualizerBars[i] = new Rectangle
                {
                    Width = 2.5,
                    Height = 8 + (i % 2) * 4,
                    Fill = barBrush,
                    RadiusX = 1,
                    RadiusY = 1,
                    Margin = new Thickness(1, 0, 1, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                _idleLeftPanel.Children.Add(_miniEqualizerBars[i]);
            }
            Grid.SetColumn(_idleLeftPanel, 0);

            // Center: Time or Countdown
            _idleCenterText = new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(_idleCenterText, 1);

            // Right: Battery %
            _idleRightPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            var batState = _batteryMonitor.GetCurrentState();
            var batText = new TextBlock
            {
                Text = batState.Percent + "%",
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            };
            var batIcon = Icons.CreatePath(Icons.Lightning, batState.IsCharging ? ThemeColors.Brush(ThemeColors.AccentGreen) : ThemeColors.Brush(ThemeColors.TextGray), null, 0, 10, 10);
            _idleRightPanel.Children.Add(batText);
            _idleRightPanel.Children.Add(batIcon);
            Grid.SetColumn(_idleRightPanel, 2);

            _idleGrid.Children.Add(_idleLeftPanel);
            _idleGrid.Children.Add(_idleCenterText);
            _idleGrid.Children.Add(_idleRightPanel);
        }
        #endregion

        #region Alert View
        private void BuildAlertView()
        {
            _alertGrid = new Grid
            {
                Margin = new Thickness(14, 0, 14, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };

            var colIcon = new ColumnDefinition { Width = GridLength.Auto };
            var colText = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            _alertGrid.ColumnDefinitions.Add(colIcon);
            _alertGrid.ColumnDefinitions.Add(colText);

            _alertIcon = Icons.CreatePath(Icons.Sparkle, ThemeColors.Brush(Settings.AccentColor), null, 0, 16, 16);
            _alertIcon.Margin = new Thickness(0, 0, 10, 0);
            Grid.SetColumn(_alertIcon, 0);

            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _alertTitle = new TextBlock
            {
                Text = "Notification",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };
            _alertSub = new TextBlock
            {
                Text = "Subtitle",
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                FontSize = 11,
                FontWeight = FontWeights.Normal
            };
            textStack.Children.Add(_alertTitle);
            textStack.Children.Add(_alertSub);
            Grid.SetColumn(textStack, 1);

            _alertGrid.Children.Add(_alertIcon);
            _alertGrid.Children.Add(textStack);
        }
        #endregion

        #region Expanded View
        private void BuildExpandedView()
        {
            _expandedGrid = new Grid
            {
                Margin = new Thickness(16, 12, 16, 12),
                Visibility = Visibility.Collapsed
            };

            var rowNav = new RowDefinition { Height = GridLength.Auto };
            var rowContent = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            _expandedGrid.RowDefinitions.Add(rowNav);
            _expandedGrid.RowDefinitions.Add(rowContent);

            // Top Navigation Bar
            var navGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            var colTabs = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var colActions = new ColumnDefinition { Width = GridLength.Auto };
            navGrid.ColumnDefinitions.Add(colTabs);
            navGrid.ColumnDefinitions.Add(colActions);

            var tabsStack = new StackPanel { Orientation = Orientation.Horizontal };
            string[] tabNames = { "🎵 Media", "📊 Stats", "⏱️ Timer", "⚡ Tools" };
            _tabButtons = new Button[tabNames.Length];
            for (int i = 0; i < tabNames.Length; i++)
            {
                int index = i;
                var btn = CreateNavButton(tabNames[i], i == 0);
                btn.Click += (s, e) => SwitchTab(index);
                _tabButtons[i] = btn;
                tabsStack.Children.Add(btn);
            }
            Grid.SetColumn(tabsStack, 0);

            // Right Action: Settings gear & Close
            var actionsStack = new StackPanel { Orientation = Orientation.Horizontal };
            var settingsBtn = CreateIconButton(Icons.Settings, "Open Control Center");
            settingsBtn.Click += (s, e) =>
            {
                if (OpenSettingsRequested != null) OpenSettingsRequested();
            };
            actionsStack.Children.Add(settingsBtn);
            Grid.SetColumn(actionsStack, 1);

            navGrid.Children.Add(tabsStack);
            navGrid.Children.Add(actionsStack);
            Grid.SetRow(navGrid, 0);
            _expandedGrid.Children.Add(navGrid);

            // Content Area
            _tabsContentGrid = new Grid();
            Grid.SetRow(_tabsContentGrid, 1);

            BuildMediaTab();
            BuildStatsTab();
            BuildTimerTab();
            BuildToolsTab();

            _tabsContentGrid.Children.Add(_mediaTabGrid);
            _tabsContentGrid.Children.Add(_statsTabGrid);
            _tabsContentGrid.Children.Add(_timerTabGrid);
            _tabsContentGrid.Children.Add(_toolsTabGrid);

            _expandedGrid.Children.Add(_tabsContentGrid);
            SwitchTab(0);
        }

        private void BuildMediaTab()
        {
            _mediaTabGrid = new Grid { Visibility = Visibility.Visible };
            var rowInfo = new RowDefinition { Height = GridLength.Auto };
            var rowControls = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            _mediaTabGrid.RowDefinitions.Add(rowInfo);
            _mediaTabGrid.RowDefinitions.Add(rowControls);

            // Track info
            var trackGrid = new Grid();
            var colArt = new ColumnDefinition { Width = GridLength.Auto };
            var colMeta = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var colWaves = new ColumnDefinition { Width = GridLength.Auto };
            trackGrid.ColumnDefinitions.Add(colArt);
            trackGrid.ColumnDefinitions.Add(colMeta);
            trackGrid.ColumnDefinitions.Add(colWaves);

            // Vinyl / Album art pill
            var albumArtBorder = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(10),
                Background = new LinearGradientBrush(
                    ThemeColors.ColorFromHex(Settings.AccentColor),
                    ThemeColors.ColorFromHex("#8A2BE2"),
                    45.0),
                Margin = new Thickness(0, 0, 12, 0)
            };
            albumArtBorder.Child = Icons.CreatePath(Icons.Music, Brushes.White, null, 0, 22, 22);
            Grid.SetColumn(albumArtBorder, 0);

            var metaStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _mediaTitleText = new TextBlock
            {
                Text = "No Media Playing",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            _mediaArtistText = new TextBlock
            {
                Text = "Open Spotify, YouTube, or VLC",
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                FontSize = 12,
                Margin = new Thickness(0, 2, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            metaStack.Children.Add(_mediaTitleText);
            metaStack.Children.Add(_mediaArtistText);
            Grid.SetColumn(metaStack, 1);

            // Large Equalizer Soundbars
            var eqStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            _largeEqualizerBars = new Rectangle[6];
            for (int i = 0; i < 6; i++)
            {
                _largeEqualizerBars[i] = new Rectangle
                {
                    Width = 3.5,
                    Height = 12 + (i % 3) * 6,
                    Fill = ThemeColors.Brush(Settings.AccentColor),
                    RadiusX = 1.5,
                    RadiusY = 1.5,
                    Margin = new Thickness(1.5, 0, 1.5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                eqStack.Children.Add(_largeEqualizerBars[i]);
            }
            Grid.SetColumn(eqStack, 2);

            trackGrid.Children.Add(albumArtBorder);
            trackGrid.Children.Add(metaStack);
            trackGrid.Children.Add(eqStack);
            Grid.SetRow(trackGrid, 0);

            // Controls Grid
            var ctrlGrid = new Grid { Margin = new Thickness(0, 12, 0, 0) };
            var colBtn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var colVol = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            ctrlGrid.ColumnDefinitions.Add(colBtn);
            ctrlGrid.ColumnDefinitions.Add(colVol);

            // Playback buttons
            var buttonsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            var prevBtn = CreateMediaControlButton(Icons.SkipPrev, 16);
            prevBtn.Click += (s, e) => MediaMonitor.SendPrevTrack();

            var playPauseBtn = CreateMediaControlButton(Icons.Play, 18, true);
            _mediaPlayPauseIcon = (Path)playPauseBtn.Content;
            playPauseBtn.Click += (s, e) =>
            {
                MediaMonitor.SendPlayPause();
                TogglePlayPauseUi();
            };

            var nextBtn = CreateMediaControlButton(Icons.SkipNext, 16);
            nextBtn.Click += (s, e) => MediaMonitor.SendNextTrack();

            buttonsStack.Children.Add(prevBtn);
            buttonsStack.Children.Add(playPauseBtn);
            buttonsStack.Children.Add(nextBtn);
            Grid.SetColumn(buttonsStack, 0);

            // Volume Control
            var volStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var volIcon = Icons.CreatePath(Icons.VolumeHigh, ThemeColors.Brush(ThemeColors.TextGray), null, 0, 14, 14);
            volIcon.Margin = new Thickness(0, 0, 8, 0);

            _volumeSlider = new Slider
            {
                Width = 110,
                Minimum = 0,
                Maximum = 100,
                Value = KeyboardHook.GetSystemVolume(),
                VerticalAlignment = VerticalAlignment.Center
            };
            volStack.Children.Add(volIcon);
            volStack.Children.Add(_volumeSlider);
            Grid.SetColumn(volStack, 1);

            ctrlGrid.Children.Add(buttonsStack);
            ctrlGrid.Children.Add(volStack);
            Grid.SetRow(ctrlGrid, 1);

            _mediaTabGrid.Children.Add(trackGrid);
            _mediaTabGrid.Children.Add(ctrlGrid);
        }

        private void BuildStatsTab()
        {
            _statsTabGrid = new Grid { Visibility = Visibility.Collapsed };
            var colLeft = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var colRight = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            _statsTabGrid.ColumnDefinitions.Add(colLeft);
            _statsTabGrid.ColumnDefinitions.Add(colRight);

            // Left: CPU & RAM
            var leftStack = new StackPanel { Margin = new Thickness(0, 0, 10, 0) };
            _cpuText = new TextBlock
            {
                Text = "CPU: 0%",
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 3)
            };
            _cpuBar = CreateGaugeBar(ThemeColors.AccentBlue);

            _ramText = new TextBlock
            {
                Text = "RAM: 0%",
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 3)
            };
            _ramBar = CreateGaugeBar(ThemeColors.AccentPurple);

            leftStack.Children.Add(_cpuText);
            leftStack.Children.Add(_cpuBar);
            leftStack.Children.Add(_ramText);
            leftStack.Children.Add(_ramBar);
            Grid.SetColumn(leftStack, 0);

            // Right: Battery & Uptime
            var rightStack = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            _batteryStatsText = new TextBlock
            {
                Text = "Battery: Checking...",
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            };
            var batCard = new Border
            {
                Background = ThemeColors.Brush(ThemeColors.CardColor),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(0, 0, 0, 6)
            };
            batCard.Child = _batteryStatsText;

            _uptimeText = new TextBlock
            {
                Text = "Uptime: 00:00:00",
                Foreground = ThemeColors.Brush(ThemeColors.TextGray),
                FontSize = 11,
                FontWeight = FontWeights.Medium
            };
            rightStack.Children.Add(batCard);
            rightStack.Children.Add(_uptimeText);
            Grid.SetColumn(rightStack, 1);

            _statsTabGrid.Children.Add(leftStack);
            _statsTabGrid.Children.Add(rightStack);
        }

        private void BuildTimerTab()
        {
            _timerTabGrid = new Grid { Visibility = Visibility.Collapsed };
            var rowDisplay = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var rowBtns = new RowDefinition { Height = GridLength.Auto };
            _timerTabGrid.RowDefinitions.Add(rowDisplay);
            _timerTabGrid.RowDefinitions.Add(rowBtns);

            _timerDisplay = new TextBlock
            {
                Text = "25:00",
                Foreground = Brushes.White,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(_timerDisplay, 0);

            var btnsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            };

            var add1m = CreatePillButton("+1m");
            add1m.Click += (s, e) => { _pomodoroRemainingSeconds += 60; UpdateTimerDisplay(); };

            var add5m = CreatePillButton("+5m");
            add5m.Click += (s, e) => { _pomodoroRemainingSeconds += 300; UpdateTimerDisplay(); };

            _timerStartBtn = CreatePillButton("Start", true);
            _timerStartBtn.Click += (s, e) => TogglePomodoro();

            var resetBtn = CreatePillButton("Reset");
            resetBtn.Click += (s, e) => ResetPomodoro();

            btnsStack.Children.Add(add1m);
            btnsStack.Children.Add(add5m);
            btnsStack.Children.Add(_timerStartBtn);
            btnsStack.Children.Add(resetBtn);
            Grid.SetRow(btnsStack, 1);

            _timerTabGrid.Children.Add(_timerDisplay);
            _timerTabGrid.Children.Add(btnsStack);
        }

        private void BuildToolsTab()
        {
            _toolsTabGrid = new Grid { Visibility = Visibility.Collapsed };
            var wrap = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var muteBtn = CreateToolCard("Mute Sound", Icons.VolumeMute, () =>
            {
                KeyboardHook.GetSystemVolume();
                // Send Mute key
                MediaMonitor.SendPlayPause(); // Toggle
            });

            var lockBtn = CreateToolCard("Lock PC", Icons.Lock, () =>
            {
                try { Process.Start("rundll32.exe", "user32.dll,LockWorkStation"); } catch { }
            });

            var snipBtn = CreateToolCard("Screenshot", Icons.Scissors, () =>
            {
                try { Process.Start("snippingtool.exe"); } catch { }
            });

            var taskBtn = CreateToolCard("Task Mgr", Icons.Cpu, () =>
            {
                try { Process.Start("taskmgr.exe"); } catch { }
            });

            wrap.Children.Add(muteBtn);
            wrap.Children.Add(lockBtn);
            wrap.Children.Add(snipBtn);
            wrap.Children.Add(taskBtn);

            _toolsTabGrid.Children.Add(wrap);
        }
        #endregion

        #region UI Helper Factories
        private Button CreateNavButton(string text, bool isActive)
        {
            var btn = new Button
            {
                Content = text,
                Foreground = isActive ? Brushes.White : ThemeColors.Brush(ThemeColors.TextGray),
                Background = isActive ? ThemeColors.Brush(ThemeColors.SubcardColor) : Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = Cursors.Hand
            };
            return btn;
        }

        private Button CreateIconButton(string iconData, string toolTip)
        {
            var btn = new Button
            {
                Content = Icons.CreatePath(iconData, ThemeColors.Brush(ThemeColors.TextGray), null, 0, 14, 14),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Width = 26,
                Height = 26,
                ToolTip = toolTip,
                Cursor = Cursors.Hand
            };
            return btn;
        }

        private Button CreateMediaControlButton(string iconData, double iconSize, bool isAccent = false)
        {
            var btn = new Button
            {
                Content = Icons.CreatePath(iconData, Brushes.White, null, 0, iconSize, iconSize),
                Width = isAccent ? 36 : 30,
                Height = isAccent ? 36 : 30,
                Background = isAccent ? ThemeColors.Brush(Settings.AccentColor) : ThemeColors.Brush(ThemeColors.CardColor),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };
            return btn;
        }

        private ProgressBar CreateGaugeBar(Color color)
        {
            var bar = new ProgressBar
            {
                Height = 6,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Foreground = ThemeColors.Brush(color),
                Background = ThemeColors.Brush(ThemeColors.CardColor),
                BorderThickness = new Thickness(0)
            };
            return bar;
        }

        private Button CreatePillButton(string text, bool isAccent = false)
        {
            var btn = new Button
            {
                Content = text,
                Foreground = Brushes.White,
                Background = isAccent ? ThemeColors.Brush(Settings.AccentColor) : ThemeColors.Brush(ThemeColors.CardColor),
                BorderThickness = new Thickness(0),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 5, 12, 5),
                Margin = new Thickness(4, 0, 4, 0),
                Cursor = Cursors.Hand
            };
            return btn;
        }

        private Border CreateToolCard(string label, string iconData, Action onClick)
        {
            var card = new Border
            {
                Background = ThemeColors.Brush(ThemeColors.CardColor),
                CornerRadius = new CornerRadius(10),
                Width = 84,
                Height = 64,
                Margin = new Thickness(5),
                Cursor = Cursors.Hand
            };
            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var icon = Icons.CreatePath(iconData, ThemeColors.Brush(Settings.AccentColor), null, 0, 18, 18);
            var text = new TextBlock
            {
                Text = label,
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 6, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(icon);
            stack.Children.Add(text);
            card.Child = stack;

            card.MouseDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) onClick(); };
            return card;
        }
        #endregion

        #region Timers & Event Wiring
        private void HookEvents()
        {
            // Hover expanding
            MouseEnter += (s, e) =>
            {
                if (Settings.ExpandOnHover && _currentState == IslandState.Idle)
                {
                    ExpandIsland();
                }
                else if (_currentState == IslandState.Expanded)
                {
                    _collapseTimer.Stop();
                }
            };

            MouseLeave += (s, e) =>
            {
                if (_currentState == IslandState.Expanded)
                {
                    _collapseTimer.Stop();
                    _collapseTimer.Interval = TimeSpan.FromSeconds(Settings.AutoCollapseSeconds);
                    _collapseTimer.Start();
                }
            };

            MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (_currentState == IslandState.Idle)
                    {
                        ExpandIsland();
                    }
                    else if (_currentState == IslandState.Alert)
                    {
                        ExpandIsland();
                    }
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    ShowContextMenu();
                }
            };

            // Keyboard and Battery events
            _keyboardHook.CapsLockChanged += isLocked =>
            {
                if (Settings.EnableCapsLock)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        TriggerAlert("Caps Lock", isLocked ? "CAPS LOCK ON" : "CAPS LOCK OFF", Icons.CapsLock, isLocked ? ThemeColors.AccentOrange : ThemeColors.TextGray);
                    }));
                }
            };

            _keyboardHook.VolumeChanged += vol =>
            {
                if (Settings.EnableVolume)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        TriggerAlert("Volume", vol + "%", Icons.VolumeHigh, ThemeColors.AccentCyan);
                    }));
                }
            };

            _batteryMonitor.BatteryEventTriggered += (bat, title) =>
            {
                if (Settings.EnableBattery)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        TriggerAlert(title, bat.Percent + "% Battery Remaining", Icons.Lightning, bat.IsCharging ? ThemeColors.AccentGreen : ThemeColors.AccentOrange);
                    }));
                }
            };

            _mediaMonitor.TrackChanged += info =>
            {
                if (Settings.EnableMedia)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        TriggerAlert("Now Playing", info.Title + " - " + info.Artist, Icons.Music, ThemeColors.AccentBlue);
                    }));
                }
            };
        }

        private void StartTimers()
        {
            // Main Poller (1.5 seconds)
            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
            _pollTimer.Tick += (s, e) => PollUpdates();
            _pollTimer.Start();

            // Collapse Timer
            _collapseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Settings.AutoCollapseSeconds) };
            _collapseTimer.Tick += (s, e) =>
            {
                _collapseTimer.Stop();
                if (_currentState != IslandState.Idle && !IsMouseOver)
                {
                    CollapseToIdle();
                }
            };

            // Equalizer Bar Animation (120ms)
            var rand = new Random();
            _equalizerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            _equalizerTimer.Tick += (s, e) =>
            {
                if (_miniEqualizerBars != null)
                {
                    for (int i = 0; i < _miniEqualizerBars.Length; i++)
                    {
                        _miniEqualizerBars[i].Height = rand.Next(4, 16);
                    }
                }
                if (_largeEqualizerBars != null && _largeEqualizerBars[0] != null)
                {
                    for (int i = 0; i < _largeEqualizerBars.Length; i++)
                    {
                        _largeEqualizerBars[i].Height = rand.Next(6, 26);
                    }
                }
            };
            _equalizerTimer.Start();

            // Pomodoro countdown timer (1s)
            _pomodoroTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _pomodoroTimer.Tick += (s, e) =>
            {
                if (_pomodoroRunning)
                {
                    if (_pomodoroRemainingSeconds > 0)
                    {
                        _pomodoroRemainingSeconds--;
                        UpdateTimerDisplay();
                        if (_currentState == IslandState.Idle)
                        {
                            _idleCenterText.Text = string.Format("{0:D2}:{1:D2}", _pomodoroRemainingSeconds / 60, _pomodoroRemainingSeconds % 60);
                        }
                    }
                    else
                    {
                        _pomodoroRunning = false;
                        _pomodoroTimer.Stop();
                        TriggerAlert("Pomodoro Timer", "Focus Session Finished! Time for a break.", Icons.Timer, ThemeColors.AccentGreen);
                    }
                }
            };
        }

        private void PollUpdates()
        {
            // Keyboard & volume
            _keyboardHook.CheckState();

            // Battery check
            _batteryMonitor.CheckForEvents();
            var bat = _batteryMonitor.GetCurrentState();
            _batteryStatsText.Text = string.Format("Battery: {0}% ({1})", bat.Percent, bat.IsCharging ? "Charging" : "Discharging");

            // Clock update in idle if pomodoro is not running
            if (!_pomodoroRunning && _currentState == IslandState.Idle)
            {
                _idleCenterText.Text = DateTime.Now.ToString("HH:mm");
            }

            // System metrics in stats tab
            if (_currentState == IslandState.Expanded && _activeTabIndex == 1)
            {
                var metrics = _sysMonitor.GetCurrentMetrics();
                _cpuBar.Value = metrics.CpuUsage;
                _cpuText.Text = string.Format("CPU: {0}%", metrics.CpuUsage);

                _ramBar.Value = metrics.RamUsage;
                _ramText.Text = string.Format("RAM: {0}% ({1:0.0} / {2:0.0} GB)", metrics.RamUsage, metrics.RamUsedGb, metrics.RamTotalGb);

                _uptimeText.Text = string.Format("Uptime: {0:D2}h {1:D2}m", (int)metrics.Uptime.TotalHours, metrics.Uptime.Minutes);
            }

            // Active media check
            var media = _mediaMonitor.PollActiveMedia();
            if (media.IsPlaying)
            {
                _mediaTitleText.Text = media.Title;
                _mediaArtistText.Text = media.Artist + " • " + media.AppName;
            }
        }
        #endregion

        #region State Animations & Morphing
        public void TriggerAlert(string title, string sub, string iconData, Color iconColor)
        {
            _alertTitle.Text = title;
            _alertSub.Text = sub;
            _alertIcon.Data = Geometry.Parse(iconData);
            _alertIcon.Fill = ThemeColors.Brush(iconColor);

            double targetWidth = Math.Max(260.0, Math.Min(340.0, (title.Length + sub.Length) * 6.5 + 80.0));
            double targetHeight = 40.0;

            SetViewState(IslandState.Alert);
            AnimateDimensions(targetWidth, targetHeight, 20.0);

            _collapseTimer.Stop();
            _collapseTimer.Interval = TimeSpan.FromSeconds(3.5);
            _collapseTimer.Start();
        }

        public void ExpandIsland()
        {
            SetViewState(IslandState.Expanded);
            AnimateDimensions(Settings.ExpandedWidth, Settings.ExpandedHeight, 22.0);
            PollUpdates(); // Refresh metrics immediately
        }

        public void CollapseToIdle()
        {
            SetViewState(IslandState.Idle);
            AnimateDimensions(Settings.IdleWidth, Settings.IdleHeight, Settings.IdleHeight / 2.0);
        }

        private void SetViewState(IslandState state)
        {
            _currentState = state;
            _idleGrid.Visibility = (state == IslandState.Idle) ? Visibility.Visible : Visibility.Collapsed;
            _alertGrid.Visibility = (state == IslandState.Alert) ? Visibility.Visible : Visibility.Collapsed;
            _expandedGrid.Visibility = (state == IslandState.Expanded) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AnimateDimensions(double targetWidth, double targetHeight, double targetRadius)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            var duration = TimeSpan.FromMilliseconds(320);

            // Calculate centered Left
            double screenW = SystemParameters.PrimaryScreenWidth;
            double targetLeft = (screenW - targetWidth) / 2.0;

            // Width animation
            var animW = new DoubleAnimation(targetWidth, duration) { EasingFunction = ease };
            // Height animation
            var animH = new DoubleAnimation(targetHeight, duration) { EasingFunction = ease };
            // Left animation
            var animL = new DoubleAnimation(targetLeft, duration) { EasingFunction = ease };

            BeginAnimation(WidthProperty, animW);
            BeginAnimation(HeightProperty, animH);
            BeginAnimation(LeftProperty, animL);

            // Border corner radius
            _mainBorder.CornerRadius = new CornerRadius(targetRadius);
        }

        public void ApplySettings(IslandSettings settings)
        {
            Settings = settings;
            Top = Settings.TopOffset;
            _mainBorder.BorderBrush = Settings.BorderGlow ? ThemeColors.Brush(Settings.AccentColor) : ThemeColors.Brush(ThemeColors.BorderDefault);

            if (_currentState == IslandState.Idle)
            {
                CollapseToIdle();
            }
        }
        #endregion

        #region Tab Switching & Controls
        private void SwitchTab(int index)
        {
            _activeTabIndex = index;
            _mediaTabGrid.Visibility = (index == 0) ? Visibility.Visible : Visibility.Collapsed;
            _statsTabGrid.Visibility = (index == 1) ? Visibility.Visible : Visibility.Collapsed;
            _timerTabGrid.Visibility = (index == 2) ? Visibility.Visible : Visibility.Collapsed;
            _toolsTabGrid.Visibility = (index == 3) ? Visibility.Visible : Visibility.Collapsed;

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                bool active = (i == index);
                _tabButtons[i].Foreground = active ? Brushes.White : ThemeColors.Brush(ThemeColors.TextGray);
                _tabButtons[i].Background = active ? ThemeColors.Brush(ThemeColors.SubcardColor) : Brushes.Transparent;
            }

            if (index == 1) PollUpdates();
        }

        private void TogglePlayPauseUi()
        {
            // Toggle local play/pause icon visual
            if (_mediaPlayPauseIcon.Data.ToString() == Icons.Play)
            {
                _mediaPlayPauseIcon.Data = Geometry.Parse(Icons.Pause);
            }
            else
            {
                _mediaPlayPauseIcon.Data = Geometry.Parse(Icons.Play);
            }
        }

        private void TogglePomodoro()
        {
            _pomodoroRunning = !_pomodoroRunning;
            if (_pomodoroRunning)
            {
                _timerStartBtn.Content = "Pause";
                _timerStartBtn.Background = ThemeColors.Brush(ThemeColors.AccentOrange);
                _pomodoroTimer.Start();
            }
            else
            {
                _timerStartBtn.Content = "Resume";
                _timerStartBtn.Background = ThemeColors.Brush(Settings.AccentColor);
                _pomodoroTimer.Stop();
            }
        }

        private void ResetPomodoro()
        {
            _pomodoroRunning = false;
            _pomodoroTimer.Stop();
            _pomodoroRemainingSeconds = 25 * 60;
            _timerStartBtn.Content = "Start";
            _timerStartBtn.Background = ThemeColors.Brush(Settings.AccentColor);
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            int m = _pomodoroRemainingSeconds / 60;
            int s = _pomodoroRemainingSeconds % 60;
            _timerDisplay.Text = string.Format("{0:D2}:{1:D2}", m, s);
        }

        private void ShowContextMenu()
        {
            var menu = new ContextMenu();

            var openSettingsItem = new MenuItem { Header = "⚙️ Dynamic Island Control Center" };
            openSettingsItem.Click += (s, e) => { if (OpenSettingsRequested != null) OpenSettingsRequested(); };

            var testAlertItem = new MenuItem { Header = "✨ Test Trigger Notification" };
            testAlertItem.Click += (s, e) => TriggerAlert("Dynamic Island", "Working smoothly on Windows!", Icons.Sparkle, ThemeColors.AccentBlue);

            var toggleSizeItem = new MenuItem { Header = (_currentState == IslandState.Expanded) ? "🔼 Collapse Island" : "🔽 Expand Island" };
            toggleSizeItem.Click += (s, e) =>
            {
                if (_currentState == IslandState.Expanded) CollapseToIdle();
                else ExpandIsland();
            };

            var exitItem = new MenuItem { Header = "❌ Exit Dynamic Island" };
            exitItem.Click += (s, e) => Application.Current.Shutdown();

            menu.Items.Add(openSettingsItem);
            menu.Items.Add(testAlertItem);
            menu.Items.Add(toggleSizeItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(exitItem);

            menu.IsOpen = true;
        }
        #endregion
    }
}
