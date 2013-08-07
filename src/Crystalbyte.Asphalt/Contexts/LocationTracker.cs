#region Using directives

using System;
using System.Composition;
using System.Data.Linq;
using System.Diagnostics;
using System.Windows;
using Crystalbyte.Asphalt.Commands;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using Windows.Devices.Geolocation;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class LocationTracker : NotificationObject {
        private double _currentLatitude;
        private double _currentLongitude;
        private bool _isTracking;

        private const double SpeedThresholdInKilometersPerSecond = 8.3;

#if DEBUG
        private const double TimeThresholdInMinutes = 0.2;
#else
        private const double TimeThresholdInMinutes = 5;
#endif

        [Import]
        public Navigation Navigation { get; set; }

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

        public Tour CurrentTour { get; private set; }
        public Geoposition LastPosition { get; private set; }
        public Geoposition CurrentPosition { get; private set; }
        public Geoposition AnchorPosition { get; private set; }
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

            Debug.WriteLine("Update received at lat:{0}, lon:{1}", position.Coordinate.Latitude,
                            position.Coordinate.Longitude);

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

        private void StartTracking() {
            NotifyStartTracking();

            if (!App.IsGeolocatorAlive) {
                Debug.WriteLine("Initializing geolocator ...");
                App.InitializeGeolocator();
            }

            CurrentTour = new Tour
                              {
                                  StartTime = DateTime.Now
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

            NotifyStopTracking();

            if (tour.Positions.Count > 0) {
                tour.CivicAddressesResolved += OnCivicAddressesResolved;
                tour.ResolveCivicAddresses();
            }

            Debug.WriteLine("Tracking stopped.");
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
            return CalculateSpeed() > SpeedThresholdInKilometersPerSecond;
        }

        private bool CheckTrackingStopCondition() {
            if (AnchorPosition == null) {
                AnchorPosition = CurrentPosition;
                return false;
            }

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
            CurrentTour.Positions.Add(new Position
                                          {
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

            var distanceInMeters = distance*1000;
            var timeElapsedInSeconds = timeElapsed.TotalSeconds;
            return distanceInMeters/timeElapsedInSeconds;
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