﻿#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class BooleanToBrushConverter : IValueConverter {
        public Brush BrushForTrue { get; set; }
        public Brush BrushForFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var b = (bool) value;
            return b ? BrushForTrue : BrushForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}