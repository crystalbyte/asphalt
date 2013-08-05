using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class AngleFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var l = (double)value;

            if (parameter is string && (string)parameter == "lat") {
                return ParseLatitude(l);
            }

            return ParseLongitude(l);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        private static string ParseLatitude(double value) {
            var direction = value < 0 ? "S" : "N";

            value = Math.Abs(value);

            var degrees = Math.Truncate(value);

            value = (value - degrees) * 60;       //not Value = (Value - degrees) / 60;

            var minutes = Math.Truncate(value);
            var seconds = Math.Round((value - minutes) * 60, 3); //not Value = (Value - degrees) / 60;

            return string.Format("{0}° {1}′ {2}″ {3}", degrees, minutes, seconds, direction);
        }

        private static string ParseLongitude(double value) {
            var direction = value < 0 ? "W" : "E";

            value = Math.Abs(value);

            var degrees = Math.Truncate(value);

            value = (value - degrees) * 60;       //not Value = (Value - degrees) / 60;

            var minutes = Math.Truncate(value);
            var seconds = Math.Round((value - minutes) * 60, 3); //not Value = (Value - degrees) / 60;

            return string.Format("{0}° {1}′ {2}″ {3}", degrees, minutes, seconds, direction);
        }
    }
}
