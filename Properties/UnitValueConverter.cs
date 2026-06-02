using System;
using System.Globalization;
using System.Windows.Data;
using Araci.Services;
using Araci.Services.Settings;

namespace Araci.Properties
{
    public sealed class UnitValueConverter : IValueConverter
    {
        public UnitKind Unit { get; set; } = UnitKind.None;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return UnitFormatter.Format(value, ObterUnidade(parameter));
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UnitKind unit = ObterUnidade(parameter);
            string text = value?.ToString() ?? string.Empty;
            string stripped = UnitFormatter.StripUnit(text, unit);
            Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (type == typeof(string))
                return stripped;

            if (type == typeof(double))
                return double.TryParse(stripped, NumberStyles.Float | NumberStyles.AllowThousands, culture, out double d) ? UnitFormatter.FromDisplay(d, unit) : Binding.DoNothing;

            if (type == typeof(float))
                return float.TryParse(stripped, NumberStyles.Float | NumberStyles.AllowThousands, culture, out float f) ? (float)UnitFormatter.FromDisplay(f, unit) : Binding.DoNothing;

            if (type == typeof(decimal))
                return decimal.TryParse(stripped, NumberStyles.Float | NumberStyles.AllowThousands, culture, out decimal m) ? (decimal)UnitFormatter.FromDisplay((double)m, unit) : Binding.DoNothing;

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
    }
}