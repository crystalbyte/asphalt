using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class BooleanToBrushConverter : IValueConverter {

        public Brush BrushForTrue { get; set; }
        public Brush BrushForFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var b = (bool)value;
            return b ? BrushForTrue : BrushForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
