#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Crystalbyte.Asphalt.Data;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class ExportStateToVisibilityConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var state = (ExportState) value;
            var inverse = parameter is string && (string) parameter == "!";

            switch (state) {
                case ExportState.Idle:
                    return inverse ? Visibility.Collapsed : Visibility.Visible;
                case ExportState.Collecting:
                case ExportState.Uploading:
                case ExportState.Completed:
                case ExportState.CompletedWithErrors:
                    return inverse ? Visibility.Visible : Visibility.Collapsed;
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