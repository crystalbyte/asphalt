using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;
using System;
using System.Composition;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class TourSelectionToggleCommand : IAppBarButtonCommand {

        [Import]
        public AppContext AppContext { get; set; }

        public TourSelectionToggleCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Manage.png", UriKind.Relative)) {
                Text = AppResources.SelectionButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                var landingPage = ((Frame)Application.Current.RootVisual).Content as LandingPage;
                return landingPage != null && landingPage.PanoramaIndex == 1;
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
            AppContext.IsSelectionEnabled = !AppContext.IsSelectionEnabled;
        }
    }
}
