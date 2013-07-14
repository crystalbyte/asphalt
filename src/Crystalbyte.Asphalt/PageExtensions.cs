using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace Crystalbyte.Asphalt {
    internal static class PageExtensions {

        private static readonly SolidColorBrush ErrorBackgroundBrush = new SolidColorBrush(Color.FromArgb(255,255,224,224));
        private static readonly Dictionary<Control, Brush> Brushes = new Dictionary<Control, Brush>();

        public static void HandleBindingValidationError(this PhoneApplicationPage phoneApplicationPage, object sender, ValidationErrorEventArgs e) {
            var control = e.OriginalSource as Control;
            if (control == null)
                return;

            switch (e.Action) {
                case ValidationErrorEventAction.Added:
                    StoreBrush(control);
                    control.Background = ErrorBackgroundBrush;
                    break;
                case ValidationErrorEventAction.Removed:
                    control.Background = Brushes[control];
                    break;
            }
        }

        private static void StoreBrush(Control control) {
            if (!Brushes.ContainsKey(control)) {
                Brushes.Add(control, control.Background);
            }
        }
    }
}
