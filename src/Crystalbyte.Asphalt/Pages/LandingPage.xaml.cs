#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Commands;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class LandingPage {
        private bool _skipNextTapEvent;

        // Constructor
        public LandingPage() {
            InitializeComponent();

            AppContext = App.Context;
            AppContext.SelectionEnabledChanged += AppContextOnSelectionEnabledChanged;
        }

        private void AppContextOnSelectionEnabledChanged(object sender, EventArgs eventArgs) {
            _skipNextTapEvent = true;
        }

        public AppContext AppContext {
            get { return DataContext as AppContext; }
            set { DataContext = value; }
        }

        public int PanoramaIndex { get; set; }

        public ICommand DeleteTourCommand {
            get { return App.Composition.GetExport<DeleteTourCommand>(); }
        }

        public ICommand DeleteDriverCommand {
            get { return App.Composition.GetExport<DeleteDriverCommand>(); }
        }

        public ICommand DeleteVehicleCommand {
            get { return App.Composition.GetExport<DeleteVehicleCommand>(); }
        }

        public Navigator Navigator {
            get { return App.Composition.GetExport<Navigator>(); }
        }

        public Channels Channels {
            get { return App.Composition.GetExport<Channels>(); }
        }

        public LocalStorage LocalStorage {
            get { return App.Composition.GetExport<LocalStorage>(); }
        }

        public TourSelectionSource TourSelectionSource {
            get { return App.Composition.GetExport<TourSelectionSource>(); }
        }

        public VehicleSelectionSource VehicleSelectionSource {
            get { return App.Composition.GetExport<VehicleSelectionSource>(); }
        }

        public DriverSelectionSource DriverSelectionSource {
            get { return App.Composition.GetExport<DriverSelectionSource>(); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.UpdateApplicationBar();
        }

        private void OnDeleteTourMenuItemClicked(object sender, RoutedEventArgs e) {
            TourSelectionSource.Selection = ((MenuItem) sender).DataContext as Tour;
            DeleteTourCommand.Execute(null);
        }

        private void OnTourTapped(object sender, GestureEventArgs e) {
            if (_skipNextTapEvent || AppContext.IsSelectionEnabled) {
                _skipNextTapEvent = false;
                return;
            }

            Tour tour;
            var success = TryFindDataContext(e.OriginalSource as DependencyObject, out tour);
            if (success) {
                HandleTourTap(tour);
            }
        }

        /// <summary>
        ///   Searches for the item's datacontext. The search will succeed if the tap was performed on a
        ///   <see cref="Microsoft.Phone.Controls.LongListMultiSelectorItem" />, else it will fail.
        /// </summary>
        /// <param name="originalSource"> The original source that registered the tap. </param>
        /// <param name="tour"> The tour context if found. </param>
        /// <returns> True on success, else false. </returns>
        private static bool TryFindDataContext<T>(DependencyObject originalSource, out T tour)
            where T : NotificationObject {
            ContentControl parent = null;
            var current = originalSource;
            while (true) {
                if (current == null) {
                    break;
                }
                current = VisualTreeHelper.GetParent(current);
                parent = current as LongListMultiSelectorItem;
                if (parent != null) {
                    break;
                }
            }

            if (parent == null) {
                tour = null;
                return false;
            }

            tour = (T) parent.DataContext;
            return true;
        }

        private void HandleTourTap(Tour tour) {
            TourSelectionSource.Selection = tour;
            Navigator.Navigate<TourDetailsPage>();
        }

        private void OnToursSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selector = (LongListMultiSelector) sender;

            var selections = TourSelectionSource.Selections;
            var prevCount = selections.Count;
            selections.Clear();

            if (selector.SelectedItems == null) {
                return;
            }

            selections.AddRange(selector.SelectedItems.Cast<Tour>());

            var postCount = selections.Count;
            if (prevCount == 0 || postCount == 0) {
                this.UpdateApplicationBar();
            }
        }

        public void OnPanoramaSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var panorama = (Panorama) sender;

            var first = e.AddedItems.Cast<PanoramaItem>().FirstOrDefault();
            if (first != null) {
                PanoramaIndex = panorama.Items.IndexOf(first);
            }

            this.UpdateApplicationBar();
        }

        private void HandleVehicleTap(Vehicle vehicle) {
            vehicle.SelectionTime = DateTime.Now;
            AppContext.RefreshSelections();

            VehicleSelectionSource.Selection = vehicle;
        }

        private void OnVehicleSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var vehicle = e.AddedItems.Cast<Vehicle>().FirstOrDefault();
            if (vehicle == null) {
                return;
            }

            HandleVehicleTap(vehicle);

            var selector = (LongListSelector) sender;
            selector.SelectedItem = null;
        }

        private void OnDriverSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var driver = e.AddedItems.Cast<Driver>().FirstOrDefault();
            if (driver == null) {
                return;
            }

            HandleDriverTap(driver);

            var selector = (LongListSelector) sender;
            selector.SelectedItem = null;
        }

        private void HandleDriverTap(Driver driver) {
            driver.SelectionTime = DateTime.Now;
            AppContext.RefreshSelections();

            DriverSelectionSource.Selection = driver;
        }

        private void OnDeleteVehicleMenuItemClicked(object sender, RoutedEventArgs e) {
            VehicleSelectionSource.Selection = ((MenuItem) sender).DataContext as Vehicle;
            DeleteVehicleCommand.Execute(null);
        }

        private void OnDeleteDriverMenuItemClicked(object sender, RoutedEventArgs e) {
            DriverSelectionSource.Selection = ((MenuItem) sender).DataContext as Driver;
            DeleteDriverCommand.Execute(null);
        }
    }
}