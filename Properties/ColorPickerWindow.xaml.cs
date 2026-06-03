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
        public ColorPickerWindow(string? initialColor)
        {
            InitializeComponent();
            PaletteColors = CreatePaletteColors();
            DataContext = this;
            HexTextBox.Text = TryNormalizeHexColor(initialColor, out string normalized)
                ? normalized
                : "#FF000000";
        }

        public IReadOnlyList<ColorSwatch> PaletteColors { get; }
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
                "#FF00FF00",
                "#FF0000FF"
            };

            double[] saturations = { 1.0, 0.85, 0.65, 0.45, 0.25 };
            double[] values = { 0.95, 0.75, 0.55, 0.35, 0.18 };

            for (int hue = 0; hue < 360; hue += 15)
            {
                foreach (double saturation in saturations)
                {
                    foreach (double value in values)
                    {
                        string hex = ToHex(HsvToColor(hue, saturation, value));

                        if (!colors.Contains(hex, StringComparer.OrdinalIgnoreCase))
                            colors.Add(hex);
                    }
                }
            }

            for (int gray = 0; gray <= 255; gray += 17)
            {
                string hex = $"#FF{gray:X2}{gray:X2}{gray:X2}";

                if (!colors.Contains(hex, StringComparer.OrdinalIgnoreCase))
                    colors.Add(hex);
            }

            return colors;
        }

        private static IReadOnlyList<ColorSwatch> CreatePaletteColors()
        {
            var swatches = new List<ColorSwatch>();

            foreach (string color in GeneratePaletteHexColors())
                swatches.Add(new ColorSwatch(color, CriarBrush(color)));

            return swatches;
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
            public ColorSwatch(string hex, Brush brush)
            {
                Hex = hex;
                Brush = brush;
            }

            public string Hex { get; }
            public Brush Brush { get; }
        }
    }
}
