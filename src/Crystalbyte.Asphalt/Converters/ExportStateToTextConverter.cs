#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class ExportStateToTextConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var state = (ExportState) value;
            switch (state) {
                case ExportState.Idle:
                    return string.Empty;
                case ExportState.Collecting:
                    return AppResources.ExportStateCollectingText;
                case ExportState.Uploading:
                    return AppResources.ExportStateUploadingText;
                case ExportState.Completed:
                    return AppResources.ExportStateCompletedText;
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