using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;

namespace Crystalbyte.Asphalt.Commands {

    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class ExportCommand : IAppBarButtonCommand {

        [Import]
        public Navigation Navigation { get; set; }

        public ExportCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Upload.png", UriKind.Relative)) {
                Text = AppResources.ExportButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                var detailsPage = ((Frame)Application.Current.RootVisual).Content as TourDetailsPage;
                var landingPage = ((Frame)Application.Current.RootVisual).Content as LandingPage;

                if (detailsPage != null) {
                    return !detailsPage.IsEditing;
                }

                return landingPage != null;
            }
        }

        public int Position {
            get { return 1; }
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        public void Execute(object parameter) {
            Navigation.Service.Navigate(new Uri(
                string.Format("/Pages/{0}.xaml", typeof(ExportPage).Name),
                UriKind.Relative));
        }
    }
}
