#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class DurationFormatter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string result;

            var time = (TimeSpan)value;
            if (time.TotalMinutes < 1) {
                result = string.Format("{0} s", Math.Round(time.TotalSeconds, 1));
            } else {
                result = time.TotalHours < 1
                    ? string.Format("{0} min", Math.Round(time.TotalMinutes, 1))
                    : string.Format("{0} h", Math.Round(time.TotalHours, 1));
            }

            var p = parameter as string;
            if (string.IsNullOrWhiteSpace(p)) {
                return result;
            }

            // Return value only
            if (p == "value") {
                result = result.Split(' ')[0];
                return result;
            }

            // Return unit only
            result = result.Split(' ')[1];
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}