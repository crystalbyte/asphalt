using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Commands;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Crystalbyte.Asphalt.Pages {
    public partial class VehicleDetailsPage {

        private const string VehicleStateKey = "vehicle";
        private bool _isNewPageInstance;

        public VehicleDetailsPage() {
            InitializeComponent();
            VehicleSelector = App.Composition.GetExport<VehicleSelectionSource>();
            Vehicle = VehicleSelector.Selection;
        }

        public VehicleSelectionSource VehicleSelector { get; set; }

        public Vehicle Vehicle {
            get { return (Vehicle)DataContext; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New) {
                State[VehicleStateKey] = Vehicle;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New) {
                Vehicle = VehicleSelector.Selection;
            }

            if (_isNewPageInstance && Vehicle == null) {
                Vehicle = (Vehicle)State[VehicleStateKey];
            }

            if (!Vehicle.IsDataLoaded) {
                Vehicle.LoadData();
            }

            Vehicle.ValidateAll();

            UpdateApplicationBar();

            _isNewPageInstance = false;
        }

        private void UpdateApplicationBar() {
            var buttonCommands = App.Composition.GetExports<IButtonCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.AddRange(buttonCommands.Select(x => x.Button));

            var menuCommands = App.Composition.GetExports<IMenuCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            ApplicationBar.MenuItems.Clear();
            ApplicationBar.MenuItems.AddRange(menuCommands.Select(x => x.MenuItem));
        }

        private void OnDeleteTourMenuItemClicked(object sender, RoutedEventArgs e) {
            var tour = (Tour)((MenuItem)sender).DataContext;
            DeleteTourAndReload(tour);
        }

        private void DeleteTourAndReload(Tour tour) {
            var localStorage = App.Composition.GetExport<LocalStorage>();

            localStorage.DataContext.Positions.DeleteAllOnSubmit(tour.Positions);
            localStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);

            localStorage.DataContext.Tours.DeleteOnSubmit(tour);
            localStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);

            Vehicle.Tours.Remove(tour);
        }
    }
}