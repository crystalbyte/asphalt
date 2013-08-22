#region Using directives

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class SpeedFormatter : IValueConverter {
        private const double MileRatio = 0.621371192;

        public AppSettings AppSettings {
            get { return App.Composition.GetExport<AppSettings>(); }
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (DesignerProperties.IsInDesignTool) {
                return value;
            }
            if (value == null) {
                return null;
            }
            var p = parameter as string;
            if (!string.IsNullOrWhiteSpace(p) && p == "ms") {
                return Format((double) value*3.6, parameter as string);
            }


            return Format((double)value, parameter as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion

        private string Format(double value, string parameter = "") {
            var result = string.Empty;
            switch (AppSettings.UnitOfLength) {
                case UnitOfLength.Kilometer:
                    result = FormatKilometerPerHour(value);
                    break;
                case UnitOfLength.Mile:
                    result = FormatMilePerHour(value);
                    break;
            }

              // Return full string
            if (string.IsNullOrWhiteSpace(parameter)) {
                return result;
            }

            // Return value only
            if (parameter == "value") {
                result = result.Split(' ')[0];
                return result;
            }

            // Return unit only
            if (parameter == "unit") {
                result = result.Split(' ')[1];
                return result;
            }

            return result;
        }

        private static string FormatMilePerHour(double value) {
            var milesPerHour = value*MileRatio;
            return string.Format("{0} mph", Math.Round(milesPerHour, 1));
        }

        private static string FormatKilometerPerHour(double value) {
            return string.Format("{0} km/h", Math.Round(value, 1));
        }
    }
}