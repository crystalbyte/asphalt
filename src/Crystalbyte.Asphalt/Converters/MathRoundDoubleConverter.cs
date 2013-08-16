using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class MathRoundDoubleConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            var digit = 1;
            var p = parameter as string;
            if (!string.IsNullOrWhiteSpace(p)) {
                digit = int.Parse(p);
            }
            
            return Math.Round((double) value, digit);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
