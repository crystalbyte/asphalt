using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class NullOrEmptyToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var p = parameter as string;
            var inverse = p != null && p == "!";

            var s = value as string;
            if (s == null) {
                return inverse ? Visibility.Visible : Visibility.Collapsed;
            }

            if (string.IsNullOrWhiteSpace(s)) {
                return inverse ? Visibility.Visible : Visibility.Collapsed;
            }
            return inverse ? Visibility.Collapsed: Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
