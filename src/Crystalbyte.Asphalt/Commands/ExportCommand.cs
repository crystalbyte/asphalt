using System.Linq;
using Microsoft.Phone.Shell;
using System;
using System.Composition;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;

namespace Crystalbyte.Asphalt.Commands {

    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class ExportCommand : IAppBarButtonCommand {

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public TourSelectionSource TourSelectionSource { get; set; }

        public ExportCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Upload.png", UriKind.Relative)) {
                Text = AppResources.ExportButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                var detailsPage = Navigator.GetCurrentPage<TourDetailsPage>();
                if (detailsPage != null) {
                    return !detailsPage.IsEditing;
                }

                var landingPage = Navigator.GetCurrentPage<LandingPage>();
                return landingPage != null 
                    && TourSelectionSource.Selections.Any() 
                    && landingPage.PanoramaIndex == 1;
            }
        }

        public int Position {
            get { return 3; }
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
            Navigator.Navigate<ExportPage>();
        }
    }
}
