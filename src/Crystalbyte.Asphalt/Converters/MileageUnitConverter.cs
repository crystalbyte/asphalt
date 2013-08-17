﻿using Crystalbyte.Asphalt.Contexts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class MileageUnitConverter : IValueConverter {

        private const double MileRatio = 0.621371192;

        public AppSettings AppSettings {
            get { return App.Composition.GetExport<AppSettings>(); }
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (AppSettings.UnitOfLength == UnitOfLength.Kilometer) {
                return value;
            }

            var v = (double)value;
            return Math.Round(v * MileRatio, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (AppSettings.UnitOfLength == UnitOfLength.Kilometer) {
                return value;
            }

            var v = (double)value;
            return Math.Round(v / MileRatio, 1);
        }

        #endregion
    }
}