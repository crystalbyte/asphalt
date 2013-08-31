using System;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class TourTypeToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) {
                return false;
            }
            var t = (TourType) value;
            switch (t) {
                case TourType.Business:
                    return true;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
