using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class TourTypeToColorConverter : IValueConverter {

        #region IValueConverter implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var t = (TourType) value;
            switch (t) {
                case TourType.Business:
                    return Color.FromArgb(255, 6, 36, 111);
                case TourType.Private:
                    return Color.FromArgb(255, 51, 5, 112);
                case TourType.Commute:
                    return Color.FromArgb(255, 0, 112, 70);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
