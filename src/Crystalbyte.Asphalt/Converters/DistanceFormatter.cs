#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class DistanceFormatter : IValueConverter {
        private const double MileRatio = 0.621371192;

        public AppSettings AppSettings {
            get { return App.Composition.GetExport<AppSettings>(); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Format((double) value);
        }

        private string Format(double value) {
            switch (AppSettings.UnitOfLength) {
                case UnitOfLength.Kilometer:
                    return FormatKilometer(value);
                case UnitOfLength.Mile:
                    return FormatMile(value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string FormatMile(double value) {
            var miles = value*MileRatio;
            return value < 1
                       ? string.Format("{0} yd", Math.Round(miles*1760, 1))
                       : string.Format("{0} mi", Math.Round(miles, 1));
        }

        private static string FormatKilometer(double value) {
            return value < 1
                       ? string.Format("{0} m", Math.Round(value*1000, 1))
                       : string.Format("{0} km", Math.Round(value, 1));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}