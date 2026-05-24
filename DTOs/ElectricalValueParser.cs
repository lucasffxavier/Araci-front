using System.Globalization;
using System.Text.RegularExpressions;

namespace Araci.DTOs
{
    public static class ElectricalValueParser
    {
        private const double DefaultKv = 12.47;

        public static double ToNumber(double value)
        {
            return value;
        }

        public static double ToNumber(string? value, double fallback = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            string text = NormalizeNumericText(value);
            Match match = Regex.Match(text, @"[-+]?\d+(\.\d+)?");

            if (match.Success &&
                double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
            {
                return parsed;
            }

            return fallback;
        }

        public static double ToVoltageKv(double value)
        {
            return value > 100 ? value / 1000 : value;
        }

        public static double ToVoltageKv(string? value, double fallback = DefaultKv)
        {
            double parsed = ToNumber(value, fallback);
            return ToVoltageKv(parsed);
        }

        private static string NormalizeNumericText(string value)
        {
            string text = value.Trim()
                .Replace(',', '.')
                .Replace("âˆ ", "∠")
                .Replace("Â°", string.Empty)
                .Replace("°", string.Empty);

            int polarIndex = text.IndexOf('∠');

            if (polarIndex >= 0)
                text = text[..polarIndex];

            return text;
        }
    }
}
