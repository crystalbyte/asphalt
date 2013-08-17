using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class SpeedFormatter : IValueConverter {

        private const double MileRatio = 0.621371192;

        public AppSettings AppSettings {
            get { return App.Composition.GetExport<AppSettings>(); }
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return null;
            }
            var p = parameter as string;
            if (!string.IsNullOrWhiteSpace(p) && p == "ms") {
                return Format((double)value * 3.6);
            }


            return Format((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion

        private string Format(double value) {
            switch (AppSettings.UnitOfLength) {
                case UnitOfLength.Kilometer:
                    return FormatKilometerPerHour(value);
                case UnitOfLength.Mile:
                    return FormatMilePerHour(value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string FormatMilePerHour(double value) {
            var milesPerHour = value * MileRatio;
            return string.Format("{0} mph", Math.Round(milesPerHour, 1));
        }

        private static string FormatKilometerPerHour(double value) {
            return string.Format("{0} km/h", Math.Round(value, 1));
        }
    }
}
