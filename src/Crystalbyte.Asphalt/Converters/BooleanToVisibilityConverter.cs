using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class BooleanToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var p = parameter as string;
            var inverse = p != null && p == "!";

            var b = (bool) value;

            if (inverse) {
                return b ? Visibility.Collapsed : Visibility.Visible;
            }

            return b ? Visibility.Visible: Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
