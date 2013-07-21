using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Pages;
using Windows.Devices.Geolocation;
using System.Windows.Threading;
using Windows.UI.Core;

namespace Crystalbyte.Asphalt.Contexts {

    [Export]
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

        public Tour CurrentTour { get; set; }
        public Geoposition LastPosition { get; set; }
        public Geoposition CurrentPosition { get; set; }
        public Geoposition AnchorPosition { get; set; }

        public event EventHandler Updated;

        public void OnUpdated(EventArgs e) {
            var handler = Updated;
            if (handler != null) handler(this, e);
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

            if (IsTracking) {
                UpdateCurrentTour();
                var stop = CheckTrackingStopCondition();
                if (stop) {
                    StopTracking();
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
            IsTracking = true;
            CurrentTour = new Tour { StartTime = DateTime.Now };

            AppContext.History.Add(CurrentTour);
        }

        private void StopTracking() {
            CurrentTour.StopTime = DateTime.Now;
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
            CurrentTour.Positions.Add(CurrentPosition);
            if (!App.IsRunningInBackground) {
                CurrentLatitude = CurrentPosition.Coordinate.Latitude;
                CurrentLongitude = CurrentPosition.Coordinate.Longitude;
            }
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
    }
}
