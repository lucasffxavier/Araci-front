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
                UnitKind.LengthKilometer => "km",
                UnitKind.VoltageVolt => "V",
                UnitKind.VoltageKV => "kV",
                UnitKind.CurrentAmpere => "A",
                UnitKind.ActivePowerW => "W",
                UnitKind.ActivePowerKW => "kW",
                UnitKind.ActivePowerMW => "MW",
                UnitKind.ReactivePowerVAr => "VAr",
                UnitKind.ReactivePowerKVAr => "kVAr",
                UnitKind.ReactivePowerMVAr => "MVAr",
                UnitKind.ApparentPowerVA => "VA",
                UnitKind.ApparentPowerKVA => "kVA",
                UnitKind.ApparentPowerMVA => "MVA",
                UnitKind.Percent => "%",
                _ => string.Empty
            };
        }

        public static UnitQuantityKind GetQuantity(UnitKind unit)
        {
            return unit switch
            {
                UnitKind.LengthMeter or UnitKind.LengthKilometer => UnitQuantityKind.Length,
                UnitKind.VoltageVolt or UnitKind.VoltageKV => UnitQuantityKind.Voltage,
                UnitKind.CurrentAmpere => UnitQuantityKind.Current,
                UnitKind.ActivePowerW or UnitKind.ActivePowerKW or UnitKind.ActivePowerMW => UnitQuantityKind.ActivePower,
                UnitKind.ReactivePowerVAr or UnitKind.ReactivePowerKVAr or UnitKind.ReactivePowerMVAr => UnitQuantityKind.ReactivePower,
                UnitKind.ApparentPowerVA or UnitKind.ApparentPowerKVA or UnitKind.ApparentPowerMVA => UnitQuantityKind.ApparentPower,
                UnitKind.Percent => UnitQuantityKind.Percent,
                _ => UnitQuantityKind.None
            };
        }

        public static UnitKind GetBaseUnit(UnitQuantityKind quantity)
        {
            return quantity switch
            {
                UnitQuantityKind.Length => UnitKind.LengthMeter,
                UnitQuantityKind.Voltage => UnitKind.VoltageKV,
                UnitQuantityKind.Current => UnitKind.CurrentAmpere,
                UnitQuantityKind.ActivePower => UnitKind.ActivePowerKW,
                UnitQuantityKind.ReactivePower => UnitKind.ReactivePowerKVAr,
                UnitQuantityKind.ApparentPower => UnitKind.ApparentPowerKVA,
                UnitQuantityKind.Percent => UnitKind.Percent,
                _ => UnitKind.None
            };
        }

        public static string Format(object? value, UnitKind unit)
        {
            if (value == null)
                return string.Empty;

            string text = value switch
            {
                double d => ToDisplay(d, unit).ToString("N2", CultureInfo.CurrentCulture),
                float f => ToDisplay(f, unit).ToString("N2", CultureInfo.CurrentCulture),
                decimal m => ((decimal)ToDisplay((double)m, unit)).ToString("N2", CultureInfo.CurrentCulture),
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

        public static double ToDisplay(double baseValue, UnitKind displayUnit)
        {
            UnitKind baseUnit = GetBaseUnit(GetQuantity(displayUnit));
            return Convert(baseValue, baseUnit, displayUnit);
        }

        public static double FromDisplay(double displayValue, UnitKind displayUnit)
        {
            UnitKind baseUnit = GetBaseUnit(GetQuantity(displayUnit));
            return Convert(displayValue, displayUnit, baseUnit);
        }

        public static double Convert(double value, UnitKind from, UnitKind to)
        {
            if (from == to || from == UnitKind.None || to == UnitKind.None)
                return value;

            UnitQuantityKind fromQuantity = GetQuantity(from);
            UnitQuantityKind toQuantity = GetQuantity(to);

            if (fromQuantity == UnitQuantityKind.None || fromQuantity != toQuantity)
                return value;

            double si = value * GetFactorToSi(from);
            return si / GetFactorToSi(to);
        }

        private static double GetFactorToSi(UnitKind unit)
        {
            return unit switch
            {
                UnitKind.LengthMeter => 1.0,
                UnitKind.LengthKilometer => 1000.0,
                UnitKind.VoltageVolt => 1.0,
                UnitKind.VoltageKV => 1000.0,
                UnitKind.CurrentAmpere => 1.0,
                UnitKind.ActivePowerW => 1.0,
                UnitKind.ActivePowerKW => 1000.0,
                UnitKind.ActivePowerMW => 1000000.0,
                UnitKind.ReactivePowerVAr => 1.0,
                UnitKind.ReactivePowerKVAr => 1000.0,
                UnitKind.ReactivePowerMVAr => 1000000.0,
                UnitKind.ApparentPowerVA => 1.0,
                UnitKind.ApparentPowerKVA => 1000.0,
                UnitKind.ApparentPowerMVA => 1000000.0,
                UnitKind.Percent => 1.0,
                _ => 1.0
            };
        }
    }
}