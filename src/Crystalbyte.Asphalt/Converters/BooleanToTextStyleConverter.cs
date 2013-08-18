#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class BooleanToTextStyleConverter : IValueConverter {
        public Style StyleForTrue { get; set; }
        public Style StyleForFalse { get; set; }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var b = (bool) value;
            return b ? StyleForTrue : StyleForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}