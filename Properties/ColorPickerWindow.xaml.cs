using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Araci.Properties
{
    public partial class ColorPickerWindow : Window
    {
        private const int PalettePixelWidth = 300;
        private const int PalettePixelHeight = 230;
        private const int LuminosityPixelWidth = 24;
        private const int LuminosityPixelHeight = 230;
        private const double HexWidth = 38;
        private const double HexHeight = 44;
        private const double HexStepX = 36;
        private const double HexStepY = 32;

        private bool _isUpdating;
        private byte _alpha = 255;
        private double _hue;
        private double _saturation;
        private double _value;

        public ColorPickerWindow(string? initialColor)
        {
            InitializeComponent();

            string normalized = TryNormalizeHexColor(initialColor, out string validInitial)
                ? validInitial
                : "#FF000000";

            ApplyHexColor(normalized, updateTextFields: true);
            OriginalPreviewBorder.Background = CriarBrush(normalized);
            RenderPickerImages();
            UpdateMarkers();
        }

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
                    double hue = normalizedX * 360.0;
                    double saturation = Math.Clamp(1.0 - normalizedY * 0.18, 0.18, 1.0);
                    double value = Math.Clamp(1.0 - normalizedY * 0.58, 0.34, 1.0);
                    string hex = ToHex(HsvToColor(hue % 360.0, saturation, value));
                    AddSwatch(swatches, hex, startX + column * HexStepX, y);
                }
            }

            return swatches;
        }

        private void OnPaletteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            ApplyPalettePoint(e.GetPosition(PaletteImage));
            e.Handled = true;
        }

        private void OnPaletteMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            ApplyPalettePoint(e.GetPosition(PaletteImage));
            e.Handled = true;
        }

        private void OnLuminosityMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            ApplyLuminosityPoint(e.GetPosition(LuminosityImage));
            e.Handled = true;
        }

        private void OnLuminosityMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            ApplyLuminosityPoint(e.GetPosition(LuminosityImage));
            e.Handled = true;
        }

        private void OnHexTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating)
                return;

            if (!TryNormalizeHexColor(HexTextBox.Text, out string normalized))
            {
                OkButton.IsEnabled = false;
                ValidationText.Text = "Valor hexadecimal inválido";
                return;
            }

            ApplyHexColor(normalized, updateTextFields: true);
        }

        private void OnHsvTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating)
                return;

            if (!TryReadDouble(HueTextBox.Text, 0, 360, out double hue) ||
                !TryReadDouble(SaturationTextBox.Text, 0, 100, out double saturation) ||
                !TryReadDouble(ValueTextBox.Text, 0, 100, out double value))
                return;

            _hue = NormalizeHue(hue);
            _saturation = Math.Clamp(saturation / 100.0, 0, 1);
            _value = Math.Clamp(value / 100.0, 0, 1);
            ApplyHsvColor(updateTextFields: true);
        }

        private void OnRgbTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating)
                return;

            if (!TryReadByte(RedTextBox.Text, out byte red) ||
                !TryReadByte(GreenTextBox.Text, out byte green) ||
                !TryReadByte(BlueTextBox.Text, out byte blue))
                return;

            Color color = Color.FromArgb(_alpha, red, green, blue);
            RgbToHsv(color, out _hue, out _saturation, out _value);
            SelectedColorHex = ToHex(color);
            UpdateAllFieldsFromCurrentColor();
            RenderPickerImages();
            UpdateMarkers();
        }

        private void OnColorClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string color && TryNormalizeHexColor(color, out string normalized))
                ApplyHexColor(normalized, updateTextFields: true);
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (!TryNormalizeHexColor(HexTextBox.Text, out string normalized))
                return;

            SelectedColorHex = normalized;
            DialogResult = true;
        }

        private void ApplyPalettePoint(Point point)
        {
            double width = PaletteImage.ActualWidth > 0 ? PaletteImage.ActualWidth : PalettePixelWidth;
            double height = PaletteImage.ActualHeight > 0 ? PaletteImage.ActualHeight : PalettePixelHeight;
            double x = Math.Clamp(point.X, 0, width);
            double y = Math.Clamp(point.Y, 0, height);

            _hue = width <= 0 ? 0 : x / width * 360.0;
            _saturation = height <= 0 ? 0 : 1.0 - y / height;
            ApplyHsvColor(updateTextFields: true);
        }

        private void ApplyLuminosityPoint(Point point)
        {
            double height = LuminosityImage.ActualHeight > 0 ? LuminosityImage.ActualHeight : LuminosityPixelHeight;
            double y = Math.Clamp(point.Y, 0, height);

            _value = height <= 0 ? 0 : 1.0 - y / height;
            ApplyHsvColor(updateTextFields: true);
        }

        private void ApplyHexColor(string normalized, bool updateTextFields)
        {
            Color color = ParseHex(normalized);
            _alpha = color.A;
            RgbToHsv(color, out _hue, out _saturation, out _value);
            SelectedColorHex = ToHex(color);

            if (updateTextFields)
                UpdateAllFieldsFromCurrentColor();

            RenderPickerImages();
            UpdateMarkers();
        }

        private void ApplyHsvColor(bool updateTextFields)
        {
            Color rgb = HsvToColor(_hue, _saturation, _value);
            Color color = Color.FromArgb(_alpha, rgb.R, rgb.G, rgb.B);
            SelectedColorHex = ToHex(color);

            if (updateTextFields)
                UpdateAllFieldsFromCurrentColor();

            RenderPickerImages();
            UpdateMarkers();
        }

        private void UpdateAllFieldsFromCurrentColor()
        {
            Color color = ParseHex(SelectedColorHex);

            _isUpdating = true;
            try
            {
                OkButton.IsEnabled = true;
                ValidationText.Text = string.Empty;
                HexTextBox.Text = SelectedColorHex;
                HueTextBox.Text = Math.Round(_hue).ToString("0", CultureInfo.InvariantCulture);
                SaturationTextBox.Text = Math.Round(_saturation * 100).ToString("0", CultureInfo.InvariantCulture);
                ValueTextBox.Text = Math.Round(_value * 100).ToString("0", CultureInfo.InvariantCulture);
                RedTextBox.Text = color.R.ToString(CultureInfo.InvariantCulture);
                GreenTextBox.Text = color.G.ToString(CultureInfo.InvariantCulture);
                BlueTextBox.Text = color.B.ToString(CultureInfo.InvariantCulture);
                NewPreviewBorder.Background = CriarBrush(SelectedColorHex);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void RenderPickerImages()
        {
            PaletteImage.Source = CreatePaletteBitmap();
            LuminosityImage.Source = CreateLuminosityBitmap();
        }

        private BitmapSource CreatePaletteBitmap()
        {
            int stride = PalettePixelWidth * 4;
            var pixels = new byte[PalettePixelHeight * stride];

            for (int y = 0; y < PalettePixelHeight; y++)
            {
                double saturation = 1.0 - y / (double)(PalettePixelHeight - 1);

                for (int x = 0; x < PalettePixelWidth; x++)
                {
                    double hue = x / (double)(PalettePixelWidth - 1) * 360.0;
                    Color color = HsvToColor(hue, saturation, _value);
                    int index = y * stride + x * 4;
                    pixels[index] = color.B;
                    pixels[index + 1] = color.G;
                    pixels[index + 2] = color.R;
                    pixels[index + 3] = 255;
                }
            }

            return BitmapSource.Create(
                PalettePixelWidth,
                PalettePixelHeight,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                pixels,
                stride);
        }

        private BitmapSource CreateLuminosityBitmap()
        {
            int stride = LuminosityPixelWidth * 4;
            var pixels = new byte[LuminosityPixelHeight * stride];

            for (int y = 0; y < LuminosityPixelHeight; y++)
            {
                double value = 1.0 - y / (double)(LuminosityPixelHeight - 1);
                Color color = HsvToColor(_hue, _saturation, value);

                for (int x = 0; x < LuminosityPixelWidth; x++)
                {
                    int index = y * stride + x * 4;
                    pixels[index] = color.B;
                    pixels[index + 1] = color.G;
                    pixels[index + 2] = color.R;
                    pixels[index + 3] = 255;
                }
            }

            return BitmapSource.Create(
                LuminosityPixelWidth,
                LuminosityPixelHeight,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                pixels,
                stride);
        }

        private void UpdateMarkers()
        {
            double paletteWidth = PaletteMarkerLayer.ActualWidth > 0 ? PaletteMarkerLayer.ActualWidth : PalettePixelWidth;
            double paletteHeight = PaletteMarkerLayer.ActualHeight > 0 ? PaletteMarkerLayer.ActualHeight : PalettePixelHeight;
            double paletteX = Math.Clamp(_hue / 360.0 * paletteWidth, 0, paletteWidth);
            double paletteY = Math.Clamp((1.0 - _saturation) * paletteHeight, 0, paletteHeight);
            Canvas.SetLeft(PaletteMarker, paletteX - PaletteMarker.Width / 2.0);
            Canvas.SetTop(PaletteMarker, paletteY - PaletteMarker.Height / 2.0);

            double luminosityHeight = LuminosityMarkerLayer.ActualHeight > 0 ? LuminosityMarkerLayer.ActualHeight : LuminosityPixelHeight;
            double luminosityY = Math.Clamp((1.0 - _value) * luminosityHeight, 0, luminosityHeight);
            Canvas.SetTop(LuminosityMarker, luminosityY - LuminosityMarker.Height / 2.0);
        }

        private static bool TryReadDouble(string? text, double min, double max, out double value)
        {
            value = 0;

            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                !double.TryParse(text, NumberStyles.Float, CultureInfo.GetCultureInfo("pt-BR"), out value))
                return false;

            value = Math.Clamp(value, min, max);
            return true;
        }

        private static bool TryReadByte(string? text, out byte value)
        {
            value = 0;

            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                return false;

            value = (byte)Math.Clamp(parsed, 0, 255);
            return true;
        }

        private static double NormalizeHue(double hue)
        {
            if (double.IsNaN(hue) || double.IsInfinity(hue))
                return 0;

            hue %= 360.0;
            return hue < 0 ? hue + 360.0 : hue;
        }

        private static Color HsvToColor(double hue, double saturation, double value)
        {
            hue = NormalizeHue(hue);
            saturation = Math.Clamp(saturation, 0, 1);
            value = Math.Clamp(value, 0, 1);

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

            return Color.FromArgb(255, ToByte(r + m), ToByte(g + m), ToByte(b + m));
        }

        private static void RgbToHsv(Color color, out double hue, out double saturation, out double value)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            if (delta < 0.000001)
                hue = 0;
            else if (Math.Abs(max - r) < 0.000001)
                hue = 60.0 * (((g - b) / delta) % 6.0);
            else if (Math.Abs(max - g) < 0.000001)
                hue = 60.0 * ((b - r) / delta + 2.0);
            else
                hue = 60.0 * ((r - g) / delta + 4.0);

            if (hue < 0)
                hue += 360.0;

            saturation = max <= 0 ? 0 : delta / max;
            value = max;
        }

        private static byte ToByte(double value)
        {
            return (byte)Math.Round(Math.Clamp(value, 0, 1) * 255);
        }

        private static Color ParseHex(string color)
        {
            byte a = byte.Parse(color.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte r = byte.Parse(color.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte g = byte.Parse(color.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte b = byte.Parse(color.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return Color.FromArgb(a, r, g, b);
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

        private static SolidColorBrush CriarBrush(string color)
        {
            if (!TryNormalizeHexColor(color, out string normalized))
                normalized = "#FF000000";

            return new SolidColorBrush(ParseHex(normalized));
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