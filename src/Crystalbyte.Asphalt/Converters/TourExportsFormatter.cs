#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Resources;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class TourExportsFormatter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (DesignerProperties.IsInDesignTool) {
                return value;
            }

            var tours = value as IList<Tour>;
            if (tours == null) {
                return value;
            }

            if (tours.Count > 1) {
                return string.Format(AppResources.TourExportsFormatString, tours.Count);
            }

            return tours.Count == 0 ? AppResources.NoExportText : AppResources.SingleExportText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}