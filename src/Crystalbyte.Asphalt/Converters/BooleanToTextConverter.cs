#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Asphalt.Converters {
    public sealed class BooleanToTextConverter : DependencyObject, IValueConverter {
        public string TextForTrue {
            get { return (string) GetValue(TextForTrueProperty); }
            set { SetValue(TextForTrueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextForTrue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextForTrueProperty =
            DependencyProperty.Register("TextForTrue", typeof (string), typeof (BooleanToTextConverter),
                                        new PropertyMetadata(string.Empty));

        public string TextForFalse {
            get { return (string) GetValue(TextForFalseProperty); }
            set { SetValue(TextForFalseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextForFalse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextForFalseProperty =
            DependencyProperty.Register("TextForFalse", typeof (string), typeof (BooleanToTextConverter),
                                        new PropertyMetadata(string.Empty));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var b = (bool) value;
            return b ? TextForTrue : TextForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}