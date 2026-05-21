using System;
using System.Globalization;
using System.Windows.Data;

namespace Araci.Controls.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool selecionado = value is bool b && b;
            return selecionado ? 0.6 : 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}