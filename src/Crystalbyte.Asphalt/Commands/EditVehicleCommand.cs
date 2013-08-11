using System;
using System.Composition;
using System.Linq;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Shell;
using Crystalbyte.Asphalt.Resources;
using Crystalbyte.Asphalt.Pages;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class EditVehicleCommand : IAppBarButtonCommand {

        public EditVehicleCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Edit.png", UriKind.Relative)) {
                Text = AppResources.EditVehicleButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        [Import]
        public Navigator Navigator { get; set; }
        
        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public VehicleSelectionSource VehicleSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            AppContext.Vehicles.CollectionChanged += 
                (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IsApplicable;
        }

        public void Execute(object parameter) {
            VehicleSelectionSource.Selection = AppContext.Vehicles.First(x => x.IsSelected);
            Navigator.Navigate<VehicleCompositionPage>();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Implementation of IAppBarButtonCommand

        public ApplicationBarIconButton Button {
            get; private set;
        }

        public bool IsApplicable {
            get { 
                var page = Navigator.GetCurrentPage<LandingPage>();
                if (page != null) {
                    return page.PanoramaIndex == 2 
                        && AppContext.Vehicles.Any(x => x.IsSelected);
                }

                return false;
            }
        }

        public int Position {
            get { return 2; }
        }

        #endregion
    }
}
