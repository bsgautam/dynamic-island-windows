using System;
using System.Windows.Media;

namespace DynamicIsland.UI
{
    public static class ThemeColors
    {
        public static readonly Color BackgroundColor = Color.FromRgb(10, 10, 12);
        public static readonly Color CardColor = Color.FromRgb(28, 28, 30);
        public static readonly Color SubcardColor = Color.FromRgb(44, 44, 46);
        public static readonly Color BorderDefault = Color.FromArgb(70, 255, 255, 255);
        public static readonly Color TextWhite = Color.FromRgb(255, 255, 255);
        public static readonly Color TextGray = Color.FromRgb(160, 160, 165);

        // Standard Apple Accents
        public static readonly Color AccentBlue = Color.FromRgb(10, 132, 255);
        public static readonly Color AccentGreen = Color.FromRgb(48, 209, 88);
        public static readonly Color AccentCyan = Color.FromRgb(100, 210, 255);
        public static readonly Color AccentPurple = Color.FromRgb(191, 90, 242);
        public static readonly Color AccentOrange = Color.FromRgb(255, 159, 10);
        public static readonly Color AccentRed = Color.FromRgb(255, 69, 58);

        public static SolidColorBrush Brush(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        public static SolidColorBrush Brush(string hex)
        {
            try
            {
                var c = (Color)ColorConverter.ConvertFromString(hex);
                var b = new SolidColorBrush(c);
                b.Freeze();
                return b;
            }
            catch (Exception)
            {
                return Brush(AccentBlue);
            }
        }

        public static Color ColorFromHex(string hex)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch (Exception)
            {
                return AccentBlue;
            }
        }
    }
}
