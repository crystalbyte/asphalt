#region Using directives

using System;
using System.ComponentModel;
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
            return DesignerProperties.IsInDesignTool ? value : Format((double) value, parameter as string);
        }

        private string Format(double value, string parameter = "") {
            var result = string.Empty;
            switch (AppSettings.UnitOfLength) {
                case UnitOfLength.Kilometer:
                    result = FormatKilometer(value);
                    break;
                case UnitOfLength.Mile:
                    result = FormatMile(value);
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
            result = result.Split(' ')[1];
            return result;
        }

        private static string FormatMile(double value) {
            var miles = value*MileRatio;
            return miles < 1
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