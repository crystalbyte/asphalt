using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Pages;
using Microsoft.Phone.Shell;
using System.Composition;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Resources;

namespace Crystalbyte.Asphalt.Commands {

    [Export(typeof(IButtonCommand))]
    public sealed class StartTrackingCommand : IButtonCommand {

        [Import]
        public LocationTracker LocationTracker { get; set; }

        [Import]
        public Navigation Navigation { get; set; }

        [Import]
        public VehicleSelectionSource VehicleSelectionSource { get; set; }

        public StartTrackingCommand() {
            Button = new ApplicationBarIconButton(new Uri(@"Assets/ApplicationBar/Transport.Play.png", UriKind.Relative)) {
                Text = AppResources.TrackingButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        public bool CanExecute(object parameter) {
            return !LocationTracker.IsTracking;
        }

        public void Execute(object parameter) {
            LocationTracker.StartTrackingManually(VehicleSelectionSource.Selection);
            Navigation.Service.Navigate(new Uri(string.Format("/Pages/{0}.xaml", typeof(LocationTrackingPage).Name), UriKind.Relative));
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
                return ((Frame)Application.Current.RootVisual).Content.GetType() == typeof(VehicleDetailsPage);
            }
        }

        public int Position {
            get { return 0; }
        }
    }
}
