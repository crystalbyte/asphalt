using System;
using System.Composition;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class AddVehicleCommand : IAppBarButtonCommand {

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public VehicleSelectionSource VehicleSelector { get; set; }

        public AddVehicleCommand() {
            Button = new ApplicationBarIconButton(new Uri(@"Assets/ApplicationBar/Add.png", UriKind.Relative)) {
                Text = AppResources.AddVehicleButtonText,
            };
            Button.Click += (sender, e) => Execute(this);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                var page = Navigator.GetCurrentPage<LandingPage>();
                return page != null && page.PanoramaIndex == 2;
            }
        }

        public int Position { get { return 0; } }

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
            VehicleSelector.Selection = new Vehicle();
            Navigator.Navigate<VehicleCompositionPage>("new");
        }
    }
}

