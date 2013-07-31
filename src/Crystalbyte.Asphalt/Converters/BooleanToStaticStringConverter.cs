using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class BooleanToStaticStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var b = (bool) value;
            return b ? TextForTrue : TextForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

        public string TextForTrue { get; set; }
        public string TextForFalse { get; set; }
    }
}
