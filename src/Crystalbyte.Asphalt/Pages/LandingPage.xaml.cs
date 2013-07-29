using System;
using System.Data.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Commands;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Controls;
using Crystalbyte.Asphalt.Data;

namespace Crystalbyte.Asphalt.Pages {
    public partial class LandingPage {
        private bool _isNewPageInstance;

        // Constructor
        public LandingPage() {
            InitializeComponent();
            DataContext = App.Context;

            _isNewPageInstance = true;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (_isNewPageInstance) {
                var navigation = App.Composition.GetExport<Navigation>();
                navigation.Initialize(NavigationService);    
            }

            ClearPreviousSelection();

            UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }

            _isNewPageInstance = false;
        }

        private void ClearPreviousSelection() {
            VehicleListSelector.SelectedItem = null;
        }

        private void UpdateApplicationBar() {
            var buttonCommands = App.Composition.GetExports<IButtonCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.AddRange(buttonCommands.Select(x => x.Button));

            var menuCommands = App.Composition.GetExports<IMenuCommand>()
                .Where(x => x.IsApplicable)
                .OrderBy(x => x.Position);

            ApplicationBar.MenuItems.Clear();
            ApplicationBar.MenuItems.AddRange(menuCommands.Select(x => x.MenuItem));
        }

        private void OnDeleteMenuItemClicked(object sender, RoutedEventArgs e) {
            var vehicle = (Vehicle) ((MenuItem) sender).DataContext;
            DeleteVehicleAndReload(vehicle);
        }

        private async static void DeleteVehicleAndReload(Vehicle vehicle) {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            localStorage.DataContext.Vehicles.DeleteOnSubmit(vehicle);
            localStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);

            if (!string.IsNullOrWhiteSpace(vehicle.ImageName)) {
                await localStorage.DeleteImageAsync(vehicle.ImageName);
            }

            // Force data reload
            App.Context.LoadData();
        }

        private void OnVehicleSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0 || !e.AddedItems.OfType<Vehicle>().Any()) {
                return;
            }

            App.Composition.GetExport<VehicleSelectionSource>().Selection = e.AddedItems.OfType<Vehicle>().First();
            NavigationService.Navigate(new Uri(string.Format("/Pages/{0}.xaml", typeof(VehicleDetailsPage).Name), UriKind.Relative));
        }
    }
}