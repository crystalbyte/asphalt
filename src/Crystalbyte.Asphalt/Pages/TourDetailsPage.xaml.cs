using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using System.ComponentModel;

namespace Crystalbyte.Asphalt.Pages {
    public partial class TourDetailsPage {

        private const string TourStateKey = "tour";
        private bool _isNewPageInstance;

        public TourDetailsPage() {
            InitializeComponent();
            if (!DesignerProperties.IsInDesignTool) {
                TourSelector = App.Composition.GetExport<TourSelectionSource>();    
            }
        }

        // [Import]
        protected TourSelectionSource TourSelector { get; set; }

        /// <summary>
        /// Set or gets the current route displayed on the map.
        /// </summary>
        public MapRoute CurrentRoute { get; private set; }

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

            if (!Tour.IsDataLoaded) {
                Tour.LoadData();
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
            var positions = TourSelector.Selection.Positions;

            if (positions.Count < 2) {
                return;
            }

            var query = QueryPool.RequestRouteQuery(
                new List<GeoCoordinate>(
                    positions.Select(x => new GeoCoordinate(x.Latitude, x.Longitude))));

            var route = await query.ExecuteAsync();
            QueryPool.Drop(query);

            UpdateMapRoute(route);
            CenterMap(positions);
        }

        private void UpdateMapRoute(Route route) {
            if (CurrentRoute != null) {
                TourMap.RemoveRoute(CurrentRoute);
            }

            var mapRoute = new MapRoute(route);
            CurrentRoute = mapRoute;
            TourMap.AddRoute(mapRoute);
        }

        private void CenterMap(IList<Position> positions) {
            var centralIndex = positions.Count / 2;
            var center = positions[centralIndex];
            TourMap.Center = new GeoCoordinate(center.Latitude, center.Longitude);
        }
    }
}