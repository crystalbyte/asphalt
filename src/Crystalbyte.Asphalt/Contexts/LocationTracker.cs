#region Using directives

using System;
using System.Composition;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Crystalbyte.Asphalt.Commands;
using Crystalbyte.Asphalt.Converters;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using Windows.Devices.Geolocation;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class LocationTracker : NotificationObject {
        private double _currentLatitude;
        private double _currentLongitude;
        private double _currentSpeed;
        private bool _isTracking;
        private int _speedExceedances;

        private static readonly AngleFormatter AngleFormatter = new AngleFormatter();

        public LocationTracker() {
            App.GeolocatorTombstoned += OnGeolocatorTombstoned;
        }

        private void OnGeolocatorTombstoned(object sender, EventArgs e) {
            if (IsTracking) {
                StopTracking();
            }
        }

        #region Imports

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public Channels Channels { get; set; }

        [Import]
        public StartTrackingCommand StartTrackingCommand { get; set; }

        [Import]
        public StopTrackingCommand StopTrackingCommand { get; set; }

        #endregion

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            AppSettings.SettingsChanged += AppSettingsOnIsMovementDetectionEnabledChanged;
        }

        private void AppSettingsOnIsMovementDetectionEnabledChanged(object sender, EventArgs eventArgs) {
            if (!IsTracking) {
                ResetState();
            }
        }

        public Tour CurrentTour { get; private set; }
        public Geoposition LastPosition { get; private set; }
        public Geoposition CurrentPosition { get; private set; }
        public Geoposition AnchorPosition { get; private set; }
        public bool IsLaunchedManually { get; private set; }

        public event EventHandler TourStored;

        public void OnTourStored(EventArgs e) {
            var handler = TourStored;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler VehicleUpdated;

        public void OnVehicleUpdated(EventArgs e) {
            var handler = VehicleUpdated;
            if (handler != null)
                handler(this, e);
        }

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

            if (LastPosition == null) {
                LastPosition = position;
                return;
            }

            var speedInMs = CalculateSpeed();
            var sane = PerformSanityCheck(speedInMs);
            if (!sane) {
                LastPosition = position;
                return;
            }

            if (!App.IsRunningInBackground) {
                CurrentSpeed = Speed.GetKmFromMs(speedInMs);
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
            }
            else {
                var success = CheckTrackingStartCondition();
                if (success) {
                    _speedExceedances += 1;
                    if (_speedExceedances >= AppSettings.SpeedExceedances) {
                        StartTracking();
                        _speedExceedances = 0;
                    }
                }
                else {
                    _speedExceedances = 0;
                }
            }

            LastPosition = position;
        }

        private static bool PerformSanityCheck(double speedInMs) {
            return speedInMs < 70.0d && speedInMs >= 0.0d;
        }

        private void StartTracking() {
            NotifyStartTracking();

            if (!App.IsGeolocatorAlive) {
                Debug.WriteLine("Initializing geolocator ...");
                App.InitializeGeolocator();
            }

            var vehicle = AppContext.Vehicles.First(x => x.IsSelected);
            Debug.Assert(vehicle != null);

            var driver = AppContext.Drivers.First(x => x.IsSelected);
            Debug.Assert(driver != null);

            CurrentTour = new Tour
                              {
                                  StartTime = DateTime.Now,
                                  VehicleId = vehicle.Id,
                                  DriverId = driver.Id,
                                  InitialMileage = vehicle.Mileage
                              };

            Debug.WriteLine("Submitting tour (Id = {0}) ...", CurrentTour.Id);

            OnTrackingStarted(EventArgs.Empty);
        }

        private void NotifyStartTracking() {
            SmartDispatcher.InvokeAsync(() => IsTracking = true);
        }

        private async void StopTracking() {
            if (CurrentTour.Positions.Count < 2) {
                var caption = AppResources.InsufficientDataCaption;
                var message = AppResources.InsufficientDataMessage;
                MessageBox.Show(message, caption, MessageBoxButton.OK);
                NotifyStopTracking();
                return;
            }

            var tour = CurrentTour;
            Debug.WriteLine("Stopping tracking ...");
            tour.StopTime = DateTime.Now;

            Debug.WriteLine("Calculating distance ...");
            tour.CalculateDistance();

            tour.UniqueId = Guid.NewGuid();

            await Channels.Database.Enqueue(() => {
                                                LocalStorage.DataContext.Tours.InsertOnSubmit(CurrentTour);
                                                LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
                                            });

            Debug.WriteLine("Submitting positions ...");
            foreach (var pos in tour.Positions) {
                pos.TourId = tour.Id;
            }

            await Channels.Database.Enqueue(() => {
                                                LocalStorage.DataContext.Positions.InsertAllOnSubmit(tour.Positions);
                                                LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
                                            });

            Debug.WriteLine("Changes successfully submitted.");

            OnTourStored(EventArgs.Empty);

            tour.ActiveVehicle.Mileage = tour.InitialMileage + tour.Distance;
            OnVehicleUpdated(EventArgs.Empty);

            NotifyStopTracking();

            if (tour.Positions.Count > 0) {
                tour.CivicAddressesResolved += OnCivicAddressesResolved;
                tour.ResolveCivicAddresses();
            }

            Debug.WriteLine("Tracking stopped.");

            if (!AppSettings.IsMovementDetectionEnabled) {
                App.TombstoneGeolocator();
            }
        }

        private void ResetState() {
            LastPosition = null;
            CurrentPosition = null;
        }

        private async void OnCivicAddressesResolved(object sender, EventArgs e) {
            var tour = (Tour) sender;
            tour.CivicAddressesResolved -= OnCivicAddressesResolved;
            await Channels.Database.Enqueue(() =>
                                            LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict));
        }

        private void NotifyStopTracking() {
            SmartDispatcher.InvokeAsync(() => IsTracking = false);
        }

        private bool CheckTrackingStartCondition() {
            var speed = CalculateSpeed();
            return speed > AppSettings.SpeedThreshold;
        }

        private bool CheckTrackingStopCondition() {
            if (AnchorPosition == null) {
                AnchorPosition = CurrentPosition;
                return false;
            }

            var speed = CalculateSpeed();
            if (speed > AppSettings.SpeedThreshold) {
                AnchorPosition = CurrentPosition;
                return false;
            }

            var current = CurrentPosition.Coordinate.Timestamp;
            var anchor = AnchorPosition.Coordinate.Timestamp;
            return current.Subtract(anchor).TotalMinutes > AppSettings.RecordingTimeout;
        }

        private void UpdateCurrentTour() {
            CurrentTour.Positions.Add(new Position
                                          {
                                              TimeStamp = CurrentPosition.Coordinate.Timestamp.Date,
                                              Latitude = CurrentPosition.Coordinate.Latitude,
                                              Longitude = CurrentPosition.Coordinate.Longitude
                                          });

            if (App.IsRunningInBackground)
                return;

            SmartDispatcher.InvokeAsync(() => {
                                            var current = CurrentPosition;
                                            if (current == null) {
                                                return;
                                            }

                                            CurrentLatitude = CurrentPosition.Coordinate.Latitude;
                                            CurrentLongitude = CurrentPosition.Coordinate.Longitude;
                                        });
        }

        private double CalculateSpeed() {
            var distance = Haversine.Delta(CurrentPosition.Coordinate, LastPosition.Coordinate);

            var lLat = LastPosition.Coordinate.Latitude;
            var lLon = LastPosition.Coordinate.Longitude;

            Debug.WriteLine("LastPosition: {0} - {1}",
                            AngleFormatter.Convert(lLat, typeof (string), "lat", null),
                            AngleFormatter.Convert(lLon, typeof (string), "lon", null));

            var cLat = CurrentPosition.Coordinate.Latitude;
            var cLon = CurrentPosition.Coordinate.Longitude;

            Debug.WriteLine("CurrentPosition: {0} - {1}",
                            AngleFormatter.Convert(cLat, typeof (string), "lat", null),
                            AngleFormatter.Convert(cLon, typeof (string), "lon", null));

            Debug.WriteLine("Distance: {0} km", distance);

            var timeElapsed = CurrentPosition.Coordinate.Timestamp.Subtract(LastPosition.Coordinate.Timestamp);

            var distanceInMeters = distance*1000;
            var timeElapsedInSeconds = timeElapsed.TotalSeconds;
            var speed = distanceInMeters/timeElapsedInSeconds;

            Debug.WriteLine("Speed: {0} m/s", speed);
            Debug.WriteLine("******************************************");
            return speed;
        }

        public double CurrentLatitude {
            get { return _currentLatitude; }
            set {
                if (Math.Abs(_currentLatitude - value) < double.Epsilon) {
                    return;
                }
                RaisePropertyChanging(() => CurrentLatitude);
                _currentLatitude = value;
                RaisePropertyChanged(() => CurrentLatitude);
            }
        }

        public double CurrentSpeed {
            get { return _currentSpeed; }
            set {
                if (Math.Abs(_currentSpeed - value) < double.Epsilon) {
                    return;
                }
                RaisePropertyChanging(() => CurrentSpeed);
                _currentSpeed = value;
                RaisePropertyChanged(() => CurrentSpeed);
            }
        }

        public double CurrentLongitude {
            get { return _currentLongitude; }
            set {
                if (Math.Abs(_currentLongitude - value) < double.Epsilon) {
                    return;
                }
                RaisePropertyChanging(() => CurrentLongitude);
                _currentLongitude = value;
                RaisePropertyChanged(() => CurrentLongitude);
            }
        }

        internal void StartTrackingManually() {
            IsLaunchedManually = true;
            StartTracking();
        }

        internal void StopTrackingManually() {
            StopTracking();
            IsLaunchedManually = false;
        }
    }
}