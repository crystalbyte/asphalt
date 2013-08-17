using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Resources;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class MileageTextConverter : IValueConverter {

        public AppSettings AppSettings {
            get { return App.Composition.GetExport<AppSettings>(); }
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return AppSettings.UnitOfLength == UnitOfLength.Mile ? value : AppResources.KilometreLabel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
