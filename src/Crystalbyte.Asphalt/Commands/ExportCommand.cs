#region Using directives

using System;
using System.Composition;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof (IAppBarButtonCommand))]
    public sealed class ExportCommand : IAppBarButtonCommand {
        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public TourSelectionSource TourSelectionSource { get; set; }

        public ExportCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Upload.png", UriKind.Relative))
                         {
                             Text = AppResources.ExportButtonText
                         };
            Button.Click += (sender, e) => Execute(null);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                // Don't display export button if there is nothing to export.
                if (AppContext.IsDataLoaded && AppContext.Tours.Count == 0) {
                    return false;
                }

                var detailsPage = Navigator.GetCurrentPage<TourDetailsPage>();
                if (detailsPage != null) {
                    return !detailsPage.IsEditing;
                }

                var landingPage = Navigator.GetCurrentPage<LandingPage>();
                return landingPage != null
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