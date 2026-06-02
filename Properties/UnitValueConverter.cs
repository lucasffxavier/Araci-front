using System;
using System.Globalization;
using System.Windows.Data;
using Araci.Services;
using Araci.Services.Settings;

namespace Araci.Properties
{
    public sealed class UnitValueConverter : IValueConverter
    {
        public static UnitDisplaySettings CurrentUnits { get; set; } = new UnitDisplaySettings();

        public UnitKind Unit { get; set; } = UnitKind.None;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UnitKind baseUnit = ObterUnidade(parameter);
            UnitKind displayUnit = ObterUnidadeExibicao(baseUnit);
            return UnitFormatter.Format(value, baseUnit, displayUnit);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UnitKind baseUnit = ObterUnidade(parameter);
            UnitKind displayUnit = ObterUnidadeExibicao(baseUnit);
            string text = value?.ToString() ?? string.Empty;
            string stripped = UnitFormatter.StripUnit(text, displayUnit);
            Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (type == typeof(string))
                return stripped;

            if (type == typeof(double))
                return double.TryParse(stripped, NumberStyles.Float | NumberStyles.AllowThousands, culture, out double d) ? UnitFormatter.FromDisplay(d, displayUnit, baseUnit) : Binding.DoNothing;

            if (type == typeof(float))
                return float.TryParse(stripped, NumberStyles.Float | NumberStyles.AllowThousands, culture, out float f) ? (float)UnitFormatter.FromDisplay(f, displayUnit, baseUnit) : Binding.DoNothing;

            if (type == typeof(decimal))
                return decimal.TryParse(stripped, NumberStyles.Float | NumberStyles.AllowThousands, culture, out decimal m) ? (decimal)UnitFormatter.FromDisplay((double)m, displayUnit, baseUnit) : Binding.DoNothing;

            if (type == typeof(int))
                return int.TryParse(stripped, NumberStyles.Integer, culture, out int i) ? i : Binding.DoNothing;

            try
            {
                return System.Convert.ChangeType(stripped, type, culture);
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        private UnitKind ObterUnidade(object parameter)
        {
            if (parameter is UnitKind unit)
                return unit;

            if (parameter is string text && Enum.TryParse(text, true, out UnitKind parsed))
                return parsed;

            return Unit;
        }

        private UnitKind ObterUnidadeExibicao(UnitKind baseUnit)
        {
            if (baseUnit == UnitKind.None)
                return UnitKind.None;

            UnitQuantityKind quantity = UnitFormatter.GetQuantity(baseUnit);
            return CurrentUnits.Resolve(quantity);
        }
    }
}
