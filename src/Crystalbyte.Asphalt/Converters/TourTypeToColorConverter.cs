﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystalbyte.Asphalt.Converters {
    public sealed class TourTypeToColorConverter : IValueConverter {

        private static readonly Brush CommuteBrush = new SolidColorBrush(Color.FromArgb(255, 69, 201, 153));
        private static readonly Brush PrivateBrush = new SolidColorBrush(Color.FromArgb(255, 210, 215, 163));
        private static readonly Brush BusinessBrush = new SolidColorBrush(Color.FromArgb(255, 86, 156, 214));

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var t = (TourType) value;
            switch (t) {
                case TourType.Business:
                    return BusinessBrush;
                case TourType.Private:
                    return PrivateBrush;
                case TourType.Commute:
                    return CommuteBrush;
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
