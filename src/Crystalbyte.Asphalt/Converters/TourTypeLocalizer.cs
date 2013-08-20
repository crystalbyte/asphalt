using Crystalbyte.Asphalt.Resources;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class TourTypeLocalizer : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var type = (TourType)value;
            switch (type) {
                case TourType.Business:
                    return AppResources.Business;
                case TourType.Private:
                    return AppResources.Private;
                case TourType.Commute:
                    return AppResources.Commute;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
