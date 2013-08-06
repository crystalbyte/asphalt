using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using System.ComponentModel;
using System.Windows;

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
        protected TourSelectionSource TourSelector {
            get {
                return App.Composition.GetExport<TourSelectionSource>();
            }
        }

        public Tour Tour {
            get { return (Tour)DataContext; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New) {
                State[TourStateKey] = Tour;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (DesignerProperties.IsInDesignTool) {
                return;
            }

            if (e.NavigationMode == NavigationMode.New) {
                Tour = TourSelector.Selection;
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
                const string message = "Unable to display route.";
                MessageBox.Show(message, ex.Message, MessageBoxButton.OK);
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

        private void OnChangeTypeButtonClicked(object sender, RoutedEventArgs e) {
            
        }

        private void OnChangeReasonButtonClicked(object sender, RoutedEventArgs e) {
            ReasonInputPrompt.IsOpen = true;
        }
    }
}