using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Maps.Controls;

namespace Crystalbyte.Asphalt.Pages {
    public partial class LocationTrackingPage {
        public LocationTrackingPage() {
            InitializeComponent();
            LocationTracker = App.Context.LocationTracker;
            LocationTracker.Updated += OnUpdated;
            LocationTracker.IsTrackingChanged += OnIsTrackingChanged;
        }

        private void CenterMap() {
            var current = LocationTracker.CurrentPosition.Coordinate;
            TourMap.Center = new GeoCoordinate(current.Latitude, current.Longitude);
        }

        private void OnUpdated(object sender, EventArgs e) {
            if (App.IsRunningInBackground)
                return;

            Dispatcher.BeginInvoke(() => {
                CenterMap();
                RequestRoute();
            });
        }

        private void RequestRoute() {
            var positions = LocationTracker.CurrentTour.Positions;
            if (positions.Count < 2) {
                return;
            }

            var query = new RouteQuery {
                Waypoints =
                    new List<GeoCoordinate>(
                        positions.Select(
                            x => new GeoCoordinate(x.Coordinate.Latitude, x.Coordinate.Longitude)))
            };
            query.QueryCompleted += OnRouteQueryCompleted;
            query.QueryAsync();
        }

        private void OnRouteQueryCompleted(object sender, QueryCompletedEventArgs<Route> e) {
            var query = (RouteQuery)sender;
            query.QueryCompleted -= OnRouteQueryCompleted;
            query.Dispose();

            UpdateMapRoute(e.Result);

        }

        private void UpdateMapRoute(Route route) {
            if (CurrentRoute != null) {
                TourMap.RemoveRoute(CurrentRoute);
            }

            CurrentRoute = new MapRoute(route);
            TourMap.AddRoute(CurrentRoute);
        }

        private void OnIsTrackingChanged(object sender, EventArgs e) {
            Dispatcher.BeginInvoke(() => {
                if (LocationTracker.IsTracking) {
                    CenterMap();
                }
            });
        }

        public MapRoute CurrentRoute { get; set; }

        public LocationTracker LocationTracker {
            get { return DataContext as LocationTracker; }
            set { DataContext = value; }
        }
    }
}