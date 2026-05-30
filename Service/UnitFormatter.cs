using System;
using System.Globalization;

namespace Araci.Services
{
    public static class UnitFormatter
    {
        public static string GetSymbol(UnitKind unit)
        {
            return unit switch
            {
                UnitKind.LengthMeter => "m",
                UnitKind.VoltageKV => "kV",
                UnitKind.CurrentAmpere => "A",
                UnitKind.ActivePowerKW => "kW",
                UnitKind.ReactivePowerKVAr => "kVAr",
                UnitKind.ApparentPowerKVA => "kVA",
                UnitKind.Percent => "%",
                _ => string.Empty
            };
        }

        public static string Format(object? value, UnitKind unit)
        {
            if (value == null)
                return string.Empty;

            string text = value switch
            {
                double d => d.ToString("N2", CultureInfo.CurrentCulture),
                float f => f.ToString("N2", CultureInfo.CurrentCulture),
                decimal m => m.ToString("N2", CultureInfo.CurrentCulture),
                _ => value.ToString() ?? string.Empty
            };

            string symbol = GetSymbol(unit);
            return string.IsNullOrWhiteSpace(symbol) ? text : $"{text} {symbol}";
        }

        public static string StripUnit(string value, UnitKind unit)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            string symbol = GetSymbol(unit);

            if (string.IsNullOrWhiteSpace(symbol))
                return value.Trim();

            string text = value.Trim();

            if (text.EndsWith(symbol, StringComparison.OrdinalIgnoreCase))
                text = text[..^symbol.Length].Trim();

            return text;
        }
    }
}