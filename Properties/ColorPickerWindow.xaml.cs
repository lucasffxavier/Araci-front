using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Araci.Properties
{
    public partial class ColorPickerWindow : Window
    {
        private const double HexWidth = 38;
        private const double HexHeight = 44;
        private const double HexStepX = 36;
        private const double HexStepY = 32;

        public ColorPickerWindow(string? initialColor)
        {
            InitializeComponent();
            PaletteColors = CreatePaletteColors();
            PaletteWidth = CalculatePaletteWidth(PaletteColors);
            PaletteHeight = CalculatePaletteHeight(PaletteColors);
            DataContext = this;
            HexTextBox.Text = TryNormalizeHexColor(initialColor, out string normalized)
                ? normalized
                : "#FF000000";
        }

        public IReadOnlyList<ColorSwatch> PaletteColors { get; }
        public double PaletteWidth { get; }
        public double PaletteHeight { get; }
        public string SelectedColorHex { get; private set; } = "#FF000000";

        public static bool TryNormalizeHexColor(string? input, out string normalized)
        {
            normalized = "#FF000000";

            if (string.IsNullOrWhiteSpace(input))
                return false;

            string text = input.Trim();

            if (text.StartsWith("#", StringComparison.Ordinal))
                text = text[1..];

            if (text.Length == 6)
                text = "FF" + text;

            if (text.Length != 8)
                return false;

            foreach (char c in text)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }

            normalized = "#" + text.ToUpperInvariant();
            return true;
        }

        public static bool TryNormalizeColor(string? value, out string normalized)
        {
            return TryNormalizeHexColor(value, out normalized);
        }

        public static IReadOnlyList<string> GeneratePaletteHexColors()
        {
            var colors = new List<string>
            {
                "#FF000000",
                "#FFFFFFFF",
                "#FFFF0000",
                "#FFFFFF00",
                "#FF00FF00",
                "#FF00FFFF",
                "#FF0000FF",
                "#FFFF00FF"
            };

            foreach (ColorSwatch swatch in GenerateHexPaletteSwatches())
            {
                if (!colors.Contains(swatch.Hex, StringComparer.OrdinalIgnoreCase))
                    colors.Add(swatch.Hex);
            }

            return colors;
        }

        public static IReadOnlyList<int> GenerateHexPaletteRowCounts()
        {
            return new[] { 7, 8, 9, 10, 11, 12, 13, 12, 11, 10, 9, 8, 7 };
        }

        public static IReadOnlyList<ColorSwatch> GenerateHexPaletteSwatches()
        {
            var swatches = new List<ColorSwatch>();
            IReadOnlyList<int> rowCounts = GenerateHexPaletteRowCounts();
            int maxColumns = rowCounts.Max();
            double mainWidth = (maxColumns - 1) * HexStepX + HexWidth;

            for (int row = 0; row < rowCounts.Count; row++)
            {
                int columns = rowCounts[row];
                double rowWidth = (columns - 1) * HexStepX + HexWidth;
                double startX = (mainWidth - rowWidth) / 2;
                double y = row * HexStepY;

                for (int column = 0; column < columns; column++)
                {
                    double normalizedX = columns == 1 ? 0.5 : column / (double)(columns - 1);
                    double normalizedY = row / (double)(rowCounts.Count - 1);
                    double centerDistance = DistanceFromCenter(normalizedX, normalizedY);
                    double hue = normalizedX * 330.0 + normalizedY * 45.0;
                    double saturation = Math.Clamp(0.28 + centerDistance * 0.72, 0.22, 1.0);
                    double value = Math.Clamp(1.0 - centerDistance * 0.46 - normalizedY * 0.08, 0.36, 1.0);
                    string hex = ToHex(HsvToColor(hue % 360.0, saturation, value));

                    AddSwatch(swatches, hex, startX + column * HexStepX, y);
                }
            }

            return swatches;
        }

        private static IReadOnlyList<ColorSwatch> CreatePaletteColors()
        {
            return GenerateHexPaletteSwatches();
        }

        private void OnHexTextChanged(object sender, TextChangedEventArgs e)
        {
            bool valido = TryNormalizeHexColor(HexTextBox.Text, out string normalized);

            OkButton.IsEnabled = valido;
            ValidationText.Text = valido ? normalized : "Valor inválido";
            PreviewFill.Background = valido
                ? CriarBrush(normalized)
                : Brushes.Black;
        }

        private void OnColorClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string color)
                HexTextBox.Text = color;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (!TryNormalizeHexColor(HexTextBox.Text, out string normalized))
                return;

            SelectedColorHex = normalized;
            DialogResult = true;
        }

        private static Color HsvToColor(double hue, double saturation, double value)
        {
            double chroma = value * saturation;
            double x = chroma * (1 - Math.Abs((hue / 60.0 % 2) - 1));
            double m = value - chroma;
            double r;
            double g;
            double b;

            if (hue < 60)
            {
                r = chroma;
                g = x;
                b = 0;
            }
            else if (hue < 120)
            {
                r = x;
                g = chroma;
                b = 0;
            }
            else if (hue < 180)
            {
                r = 0;
                g = chroma;
                b = x;
            }
            else if (hue < 240)
            {
                r = 0;
                g = x;
                b = chroma;
            }
            else if (hue < 300)
            {
                r = x;
                g = 0;
                b = chroma;
            }
            else
            {
                r = chroma;
                g = 0;
                b = x;
            }

            return Color.FromArgb(
                255,
                ToByte(r + m),
                ToByte(g + m),
                ToByte(b + m));
        }

        private static byte ToByte(double value)
        {
            return (byte)Math.Round(Math.Clamp(value, 0, 1) * 255);
        }

        private static string ToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static PointCollection CreateHexPoints(double width, double height)
        {
            return new PointCollection
            {
                new(width / 2, 0),
                new(width, height * 0.25),
                new(width, height * 0.75),
                new(width / 2, height),
                new(0, height * 0.75),
                new(0, height * 0.25)
            };
        }

        private static void AddSwatch(List<ColorSwatch> swatches, string hex, double x, double y)
        {
            swatches.Add(new ColorSwatch(
                hex,
                CriarBrush(hex),
                x,
                y,
                HexWidth,
                HexHeight,
                CreateHexPoints(HexWidth, HexHeight)));
        }

        private static double DistanceFromCenter(double normalizedX, double normalizedY)
        {
            double dx = normalizedX - 0.5;
            double dy = normalizedY - 0.5;
            return Math.Clamp(Math.Sqrt(dx * dx + dy * dy) / Math.Sqrt(0.5), 0, 1);
        }

        private static double CalculatePaletteWidth(IReadOnlyList<ColorSwatch> swatches)
        {
            return swatches.Count == 0
                ? 0
                : swatches.Max(s => s.X + s.Width) + 2;
        }

        private static double CalculatePaletteHeight(IReadOnlyList<ColorSwatch> swatches)
        {
            return swatches.Count == 0
                ? 0
                : swatches.Max(s => s.Y + s.Height) + 2;
        }

        private static SolidColorBrush CriarBrush(string color)
        {
            byte a = byte.Parse(color.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte r = byte.Parse(color.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte g = byte.Parse(color.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte b = byte.Parse(color.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        public sealed class ColorSwatch
        {
            public ColorSwatch(string hex, Brush brush, double x, double y, double width, double height, PointCollection points)
            {
                Hex = hex;
                Brush = brush;
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Points = points;
            }

            public string Hex { get; }
            public Brush Brush { get; }
            public double X { get; }
            public double Y { get; }
            public double Width { get; }
            public double Height { get; }
            public PointCollection Points { get; }
        }
    }
}
