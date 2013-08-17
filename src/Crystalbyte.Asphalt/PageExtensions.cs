#region Using directives

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Crystalbyte.Asphalt.Commands;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt {
    internal static class PageExtensions {
        public static void UpdateApplicationBar(this PhoneApplicationPage page) {
            var buttonCommands = App.Composition.GetExports<IAppBarButtonCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            page.ApplicationBar.Buttons.Clear();
            page.ApplicationBar.Buttons.AddRange(buttonCommands.Select(x => x.Button));

            var menuCommands = App.Composition.GetExports<IAppBarMenuCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            page.ApplicationBar.MenuItems.Clear();
            page.ApplicationBar.MenuItems.AddRange(menuCommands.Select(x => x.MenuItem));

            page.ApplicationBar.IsVisible = page.ApplicationBar.MenuItems.Count > 0 ||
                                            page.ApplicationBar.Buttons.Count > 0;

            page.ApplicationBar.Mode = page.ApplicationBar.Buttons.Count > 0
                                           ? ApplicationBarMode.Default
                                           : ApplicationBarMode.Minimized;
        }

        private static readonly SolidColorBrush ErrorBackgroundBrush =
            new SolidColorBrush(Color.FromArgb(255, 255, 224, 224));

        private static readonly Dictionary<Control, Brush> Brushes = new Dictionary<Control, Brush>();

        public static void HandleBindingValidationError(this PhoneApplicationPage phoneApplicationPage, object sender,
                                                        ValidationErrorEventArgs e) {
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