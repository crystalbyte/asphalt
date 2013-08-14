#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class DistanceFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var distance = (double) value;

            return distance < 1
                       ? string.Format("{0} {1}", (Math.Round(distance * 1000, 1)), "m")
                       : string.Format("{0} {1}", (Math.Round(distance, 1)), "km");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}