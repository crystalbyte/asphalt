using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Commands;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Maps.Controls;

namespace Crystalbyte.Asphalt.Pages {
    public partial class LocationTrackingPage {
        private bool _isNewPageInstance;

        public LocationTrackingPage() {
            InitializeComponent();
            LocationTracker = App.Composition.GetExport<LocationTracker>();
            LocationTracker.Updated += OnUpdated;
            LocationTracker.IsTrackingChanged += OnIsTrackingChanged;
        }

        public LocationTracker LocationTracker {
            get { return DataContext as LocationTracker; }
            set { DataContext = value; }
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

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (_isNewPageInstance) {
                var navigation = App.Composition.GetExport<Navigation>();
                navigation.Initialize(NavigationService);
            }

            UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }

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

        private void RequestRoute() {
            var positions = LocationTracker.CurrentTour.Positions;
            var enumerable = positions as Position[] ?? positions.ToArray();
            if (enumerable.Count() < 2) {
                return;
            }

            var query = new RouteQuery {
                Waypoints =
                    new List<GeoCoordinate>(
                        enumerable.Select(
                            x => new GeoCoordinate(x.Latitude, x.Longitude)))
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
    }
}