using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class UnitOfLengthFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var u = (UnitOfLength) value;
            return u == UnitOfLength.Kilometer ? "km/h" : "mph";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
