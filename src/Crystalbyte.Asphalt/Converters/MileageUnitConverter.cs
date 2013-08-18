#region Using directives

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class MileageUnitConverter : IValueConverter {
        private const double MileRatio = 0.621371192;

        public AppSettings AppSettings {
            get { return App.Composition.GetExport<AppSettings>(); }
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (DesignerProperties.IsInDesignTool) {
                return value;
            }

            var v = (double) value;

            return AppSettings.UnitOfLength == UnitOfLength.Kilometer
                       ? Math.Round(v, 1)
                       : Math.Round(v*MileRatio, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {

            var v = double.Parse((string)value);

            return AppSettings.UnitOfLength == UnitOfLength.Kilometer
                       ? Math.Round(v, 1)
                       : Math.Round(v/MileRatio, 1);
        }

        #endregion
    }
}