using System;
using System.Collections.Generic;
using System.Composition;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Pages;
using Windows.Devices.Geolocation;
using System.Windows.Threading;
using Windows.UI.Core;

namespace Crystalbyte.Asphalt.Contexts {

    [Export, Shared]
    public sealed class LocationTracker : NotificationObject {
        private double _currentLatitude;
        private double _currentLongitude;
        private bool _isTracking;

        private const double SpeedThresholdInKilometersPerSecond = 8.3;
        private const double TimeThresholdInMinutes = 5;

        [Import]
        public Navigation Navigation { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        public Tour CurrentTour { get; private set; }
        public Geoposition LastPosition { get; private set; }
        public Geoposition CurrentPosition { get; private set; }
        public Geoposition AnchorPosition { get; private set; }
        public Vehicle CurrentVehicle { get; private set; }
        public bool IsLaunchedManually { get; private set; }

        public event EventHandler Updated;

        public void OnUpdated(EventArgs e) {
            var handler = Updated;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler TrackingStarted;

        public void OnTrackingStarted(EventArgs e) {
            var handler = TrackingStarted;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler IsTrackingChanged;

        public void OnIsTrackingChanged(EventArgs e) {
            var handler = IsTrackingChanged;
            if (handler != null)
                handler(this, e);
        }

        public bool IsTracking {
            get { return _isTracking; }
            set {
                if (_isTracking == value) {
                    return;
                }

                RaisePropertyChanging(() => IsTracking);
                _isTracking = value;
                RaisePropertyChanged(() => IsTracking);
                OnIsTrackingChanged(EventArgs.Empty);
            }
        }

        public void Update(Geoposition position) {
            CurrentPosition = position;

            Debug.WriteLine("Update received at lat:{0}, lon:{1}", position.Coordinate.Latitude, position.Coordinate.Longitude);

            if (LastPosition == null) {
                LastPosition = position;
                return;
            }

            if (IsTracking) {
                UpdateCurrentTour();

                // Manual launches can only be stopped manually
                if (!IsLaunchedManually) {
                    var stop = CheckTrackingStopCondition();
                    if (stop) {
                        StopTracking();
                    }
                }

                OnUpdated(EventArgs.Empty);
                return;
            }

            var start = CheckTrackingStartCondition();
            if (start) {
                StartTracking();
            }

            LastPosition = position;
        }

        public void StartTrackingManually(Vehicle vehicle) {
            IsLaunchedManually = true;
            StartTracking(vehicle);
        }

        private void StartTracking(Vehicle vehicle = null) {
            IsTracking = true;

            CurrentVehicle = vehicle;

            Debug.WriteLine("Tracking started ...");
            if (vehicle != null) {
                Debug.WriteLine("Associated vehicle found (Id = {0}, LicencePlate = {1}).", vehicle.Id, vehicle.LicencePlate);
            } else {
                Debug.WriteLine("No associated vehicle found.");
            }

            if (!App.IsGeolocatorAlive) {
                Debug.WriteLine("Initializing geolocator ...");
                App.InitializeGeolocator();
            }

            CurrentTour = new Tour {
                StartTime = DateTime.Now
            };

            if (vehicle != null) {
                CurrentTour.VehicleId = CurrentVehicle.Id;
                CurrentVehicle.Tours.Add(CurrentTour);
            }

            Debug.WriteLine("Submitting tour (Id = {0}, VehicleId = {1}) ...", CurrentTour.Id, CurrentTour.VehicleId);
            LocalStorage.DataContext.Tours.InsertOnSubmit(CurrentTour);
            LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);

            OnTrackingStarted(EventArgs.Empty);
        }

        private void StopTracking() {
            Debug.WriteLine("Stopping tracking ...");
            CurrentTour.StopTime = DateTime.Now;

            Debug.WriteLine("Submitting positions ...");
            foreach (var pos in CurrentTour.Positions) {
                Debug.WriteLine("@: {0}", pos);
            }

            LocalStorage.DataContext.Positions.InsertAllOnSubmit(CurrentTour.Positions);
            LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
            Debug.WriteLine("Changes successfully submitted.");

            IsTracking = false;

            if (CurrentVehicle != null) {
                CurrentVehicle.InvalidateData();
            }

            // Only disable geolocator if background services are disabled
            if (AppSettings.IsBackgroundServiceEnabled) {
                return;
            }
            
            App.TombstoneGeolocator();
            Debug.WriteLine("Tombstoned geolocator.");
            Debug.WriteLine("Tracking stopped.");
        }

        private bool CheckTrackingStartCondition() {
            return CalculateSpeed() > SpeedThresholdInKilometersPerSecond;
        }

        private bool CheckTrackingStopCondition() {
            var speed = CalculateSpeed();
            if (speed > SpeedThresholdInKilometersPerSecond) {
                AnchorPosition = CurrentPosition;
                return false;
            }

            var current = CurrentPosition.Coordinate.Timestamp;
            var anchor = AnchorPosition.Coordinate.Timestamp;
            return current.Subtract(anchor).TotalMinutes > TimeThresholdInMinutes;
        }

        private void UpdateCurrentTour() {
            CurrentTour.Positions.Add(new Position {
                TourId = CurrentTour.Id,
                TimeStamp = CurrentPosition.Coordinate.Timestamp.Date,
                Latitude = CurrentPosition.Coordinate.Latitude,
                Longitude = CurrentPosition.Coordinate.Longitude
            });

            if (App.IsRunningInBackground)
                return;

            CurrentLatitude = CurrentPosition.Coordinate.Latitude;
            CurrentLongitude = CurrentPosition.Coordinate.Longitude;
        }

        private double CalculateSpeed() {
            var distance = Haversine.Delta(CurrentPosition.Coordinate, LastPosition.Coordinate);
            var timeElapsed = CurrentPosition.Coordinate.Timestamp.Subtract(LastPosition.Coordinate.Timestamp);

            var distanceInMeters = distance * 1000;
            var timeElapsedInSeconds = timeElapsed.TotalSeconds;
            return distanceInMeters / timeElapsedInSeconds;
        }

        public double CurrentLatitude {
            get { return _currentLatitude; }
            set {
                if (Math.Abs(_currentLatitude - value) < 0.1) {
                    return;
                }
                RaisePropertyChanging(() => CurrentLatitude);
                _currentLatitude = value;
                RaisePropertyChanged(() => CurrentLatitude);
            }
        }

        public double CurrentLongitude {
            get { return _currentLongitude; }
            set {
                if (Math.Abs(_currentLongitude - value) < 0.1) {
                    return;
                }
                RaisePropertyChanging(() => CurrentLongitude);
                _currentLongitude = value;
                RaisePropertyChanged(() => CurrentLongitude);
            }
        }

        public void StopTrackingManually() {
            StopTracking();
            IsLaunchedManually = false;
        }
    }
}
