using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Pages;
using Microsoft.Phone.Shell;
using System.Composition;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Resources;

namespace Crystalbyte.Asphalt.Commands {

    [Export(typeof(IButtonCommand))]
    public sealed class StopTrackingCommand : IButtonCommand {

        [Import]
        public LocationTracker LocationTracker { get; set; }

        [Import]
        public Navigation Navigation { get; set; }

        public StopTrackingCommand() {
            Button = new ApplicationBarIconButton(new Uri(@"Assets/ApplicationBar/Stop.png", UriKind.Relative)) {
                Text = AppResources.TrackingButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        public bool CanExecute(object parameter) {
            return LocationTracker.IsTracking;
        }

        public void Execute(object parameter) {
            LocationTracker.StopTrackingManually();
            if (LocationTracker.CurrentVehicle != null) {
                App.Composition.GetExport<VehicleSelectionSource>().Selection = LocationTracker.CurrentVehicle;
                Navigation.Service.Navigate(new Uri(string.Format("/Pages/{0}.xaml?page=history", typeof(VehicleDetailsPage).Name), UriKind.Relative));
            }
            else {
                Navigation.Service.Navigate(new Uri(string.Format("/Pages/{0}.xaml?page=history", typeof(LandingPage).Name), UriKind.Relative));
            }
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            LocationTracker.IsTrackingChanged += OnLocationTrackerIsTrackingChanged;
        }

        private void OnLocationTrackerIsTrackingChanged(object sender, EventArgs e) {
            OnCanExecuteChanged(EventArgs.Empty);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                return ((Frame)Application.Current.RootVisual).Content.GetType() == typeof(LocationTrackingPage);
            }
        }

        public int Position {
            get { return 0; }
        }
    }
}
