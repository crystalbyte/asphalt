#region Using directives

using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Crystalbyte.Asphalt.Resources;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class NullTextConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s)) {
                var p = (string) parameter;
                return typeof (AppResources).GetProperty(p).GetValue(null, BindingFlags.Static, null, null, null);
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}