#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Controls;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class TourDetailsPage {
        private const string TourStateKey = "tour";
        private bool _isNewPageInstance;
        private bool _routeQueryCompleted;
        private bool _isZoomed;

        public TourDetailsPage() {
            InitializeComponent();
            TourMap.ZoomLevelChanged += OnTourMapZoomLevelChanged;
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                HandleEnterKey();
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }

        private void HandleEnterKey() {
            Focus();
        }

        private void OnTourMapZoomLevelChanged(object sender, MapZoomLevelChangedEventArgs e) {
            if (_isZoomed)
                return;

            // The method "ZoomToFit(positions)" uses the "Map.SetView(bounds)" function to scale the map.
            // Unfortunately we have to wait until the map control has been properly initialized.
            // The ZoomLevelChanged event serves as the "readyness" indicator.
            // http://www.awenius.de/blog/2013/07/26/windows-phone-8-map-setview-funktioniert-erst-nach-dem-ersten-zoomlevelchanged-event/
            ZoomToFit(Tour.Positions);

            _isZoomed = true;
        }

        // [Import]
        protected TourSelectionSource TourSelectionSource {
            get { return App.Composition.GetExport<TourSelectionSource>(); }
        }

        public bool IsEditing { get; private set; }

        public Tour Tour {
            get { return (Tour)DataContext; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New) {
                State[TourStateKey] = Tour;
            }

            if (e.NavigationMode == NavigationMode.Back) {
                TourSelectionSource.Selection = null;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (DesignerProperties.IsInDesignTool) {
                return;
            }

            if (e.NavigationMode == NavigationMode.New) {
                Tour = TourSelectionSource.Selection;
            }

            if (_isNewPageInstance && Tour == null) {
                Tour = (Tour)State[TourStateKey];
            }

            Tour.ValidateAll();

            this.UpdateApplicationBar();

            // We can't launch multiple queries and have to wait for previous one's to complete.
            if (Tour.IsQuerying) {
                Tour.CivicAddressesResolved += OnTourCivicAddressesResolved;
            } else {
                RequestRoute();
            }

            _isNewPageInstance = false;
        }

        private void OnTourCivicAddressesResolved(object sender, EventArgs e) {
            Tour.CivicAddressesResolved -= OnTourCivicAddressesResolved;
            RequestRoute();
        }

        private async void RequestRoute() {
            if (!Tour.IsDataLoaded) {
                await Tour.LoadData();
            }

            var positions = Tour.Positions;

            if (positions.Count < 2 || _routeQueryCompleted) {
                return;
            }

            Tour.IsQuerying = true;

            var query = QueryPool.RequestRouteQuery(
                new List<GeoCoordinate>(
                    positions.Select(x => new GeoCoordinate(x.Latitude, x.Longitude))));

            try {
                if (Tour.CachedRoute == null) {
                    Tour.CachedRoute = await query.ExecuteAsync();
                }

                DisplayRoute(Tour.CachedRoute);

                _routeQueryCompleted = true;
            }
            catch (COMException ex) {
                const string caption = "Unable to display route.";
                MessageBox.Show(ex.Message, caption, MessageBoxButton.OK);
            }
            finally {
                QueryPool.Drop(query);
            }

            Tour.IsQuerying = false;
        }

        private void ZoomToFit(ObservableCollection<Position> positions) {
            var bounds = new LocationRectangle(
                positions.Max(p => p.Latitude),
                positions.Min(p => p.Longitude),
                positions.Min(p => p.Latitude),
                positions.Max(p => p.Longitude));
            TourMap.SetView(bounds);
        }

        private void DisplayRoute(Route route) {
            var mapRoute = new MapRoute(route);
            TourMap.AddRoute(mapRoute);
        }

        private void OnTourTypeSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var picker = (ListPicker)sender;
            Tour.Type = (TourType)picker.SelectedItem;
        }

        private void OnReasonInputFormGotFocus(object sender, RoutedEventArgs e) {
            IsEditing = true;
            ApplicationBar.IsVisible = false;
        }

        private void OnReasonInputFormLostFocus(object sender, RoutedEventArgs e) {
            IsEditing = false;
            ApplicationBar.IsVisible = true;
        }
    }
}