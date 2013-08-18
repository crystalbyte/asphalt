#region Using directives

using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class AppSettings : NotificationObject {
        private readonly IsolatedStorageSettings _isolatedStorage;
        private bool _isEditing;


        [Import]
        public LocationTracker LocationTracker { get; set; }

        public AppSettings() {
            _isolatedStorage = IsolatedStorageSettings.ApplicationSettings;
        }

        public IEnumerable<UnitOfLength> UnitOfLengthItemsSource {
            get { return Enum.GetValues(typeof (UnitOfLength)).Cast<UnitOfLength>(); }
        }

        public event EventHandler IsEditingChanged;

        public void OnIsEditingChanged(EventArgs e) {
            var handler = IsEditingChanged;
            if (handler != null)
                handler(this, e);
        }

        public bool IsEditing {
            get { return _isEditing; }
            set {
                if (_isEditing == value) {
                    return;
                }

                RaisePropertyChanging(() => IsEditing);
                _isEditing = value;
                RaisePropertyChanged(() => IsEditing);
                OnIsEditingChanged(EventArgs.Empty);
            }
        }

        public event EventHandler SettingsChanged;

        public void OnSettingsChanged(EventArgs e) {
            var handler = SettingsChanged;
            if (handler != null)
                handler(this, e);

            if (IsMovementDetectionEnabled && !App.IsGeolocatorAlive) {
                App.InitializeGeolocator();
            }
            else {
                // Don't interrupt active recording.
                if (!LocationTracker.IsTracking) {
                    // Reset geolocator to new values
                    App.TombstoneGeolocator();
                    if (IsMovementDetectionEnabled) {
                        App.InitializeGeolocator();
                    }
                }
            }
        }

        /// <summary>
        ///   Gets or sets whether the app should automatically start and stop recordings based on its heuristics.
        /// </summary>
        public bool IsMovementDetectionEnabled {
            get {
                var name = NameOf(() => IsMovementDetectionEnabled);
                if (_isolatedStorage.Contains(name)) {
                    return (bool) _isolatedStorage[name];
                }
                return true;
            }
            set {
                if (IsMovementDetectionEnabled == value) {
                    return;
                }

                var name = NameOf(() => IsMovementDetectionEnabled);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, value);
                }

                RaisePropertyChanging(() => IsMovementDetectionEnabled);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => IsMovementDetectionEnabled);

                Save();

                Debug.WriteLine("{0} = {1}", name, value);
                OnSettingsChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        ///   The unit of length used for export and displaying data.
        /// </summary>
        public UnitOfLength UnitOfLength {
            get {
                var name = NameOf(() => UnitOfLength);
                if (_isolatedStorage.Contains(name)) {
                    return (UnitOfLength) _isolatedStorage[name];
                }
                return UnitOfLength.Kilometer;
            }
            set {
                if (UnitOfLength == value) {
                    return;
                }

                var name = NameOf(() => UnitOfLength);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, value);
                }

                RaisePropertyChanging(() => UnitOfLength);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => UnitOfLength);

                Debug.WriteLine("{0} = {1}", name, value);
                Save();

                // This one is necessary to update the speed threshold label to the correct unit of length.
                RaisePropertyChanged(() => SpeedThreshold);

                // This one is necessary to update all the distance labels from all tours.
                App.Context.Tours.ForEach(x => x.UpdateUnitsOfLength());

                OnSettingsChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        ///   Gets or sets the amount of positive speed samples, before a new recording is invoked.
        /// </summary>
        public int SpeedExceedances {
            get {
                var name = NameOf(() => SpeedExceedances);
                if (_isolatedStorage.Contains(name)) {
                    return (int) _isolatedStorage[name];
                }
                return 2;
            }
            set {
                if (SpeedExceedances == value) {
                    return;
                }

                var name = NameOf(() => SpeedExceedances);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, value);
                }

                RaisePropertyChanging(() => SpeedExceedances);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => SpeedExceedances);

                Debug.WriteLine("{0} = {1}", name, value);
                Save();
            }
        }

        /// <summary>
        ///   Gets or sets the required accuracy of a GPS reading in meters, before it is passed to the location tracker.
        /// </summary>
        public double RequiredAccuracy {
            get {
                var name = NameOf(() => RequiredAccuracy);
                if (_isolatedStorage.Contains(name)) {
                    return (double) _isolatedStorage[name];
                }
                return 55.0d;
            }
            set {
                if (Math.Abs(RequiredAccuracy - value) < double.Epsilon) {
                    return;
                }

                var name = NameOf(() => RequiredAccuracy);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, value);
                }

                RaisePropertyChanging(() => RequiredAccuracy);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => RequiredAccuracy);

                Debug.WriteLine("{0} = {1}", name, value);
                Save();
            }
        }

        /// <summary>
        ///   Gets or sets the reporting interval for the Geolocator instance in milliseconds.
        /// </summary>
        public uint ReportInterval {
            get {
                var name = NameOf(() => ReportInterval);
                if (_isolatedStorage.Contains(name)) {
                    return (uint) _isolatedStorage[name];
                }
                return 2000;
            }
            set {
                if (ReportInterval == value) {
                    return;
                }

                var name = NameOf(() => ReportInterval);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, value);
                }

                RaisePropertyChanging(() => ReportInterval);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => ReportInterval);

                Debug.WriteLine("{0} = {1}", name, value);
                Save();
            }
        }

        /// <summary>
        ///   Gets or sets the timeout used to stop an active recording in minutes. 
        ///   Everytime the speed threshold is crossed the timer is reset.
        /// </summary>
        public double RecordingTimeout {
            get {
                var name = NameOf(() => RecordingTimeout);
                if (_isolatedStorage.Contains(name)) {
                    return (double) _isolatedStorage[name];
                }
                return 3.5;
            }
            set {
                if (Math.Abs(RecordingTimeout - value) < double.Epsilon) {
                    return;
                }

                var name = NameOf(() => RecordingTimeout);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, value);
                }

                RaisePropertyChanging(() => RecordingTimeout);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => RecordingTimeout);

                Debug.WriteLine("{0} = {1}", name, value);
                Save();
            }
        }

        /// <summary>
        ///   Gets or sets the speed threshold to start a new recording or reset the recording timer in m/s.
        /// </summary>
        public double SpeedThreshold {
            get {
                var name = NameOf(() => SpeedThreshold);
                if (_isolatedStorage.Contains(name)) {
                    return (double) _isolatedStorage[name];
                }
                return 7.22; // (26 km/h)
            }
            set {
                if (Math.Abs(SpeedThreshold - value) < double.Epsilon) {
                    return;
                }

                var name = NameOf(() => SpeedThreshold);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, value);
                }

                RaisePropertyChanging(() => SpeedThreshold);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => SpeedThreshold);

                Debug.WriteLine("{0} = {1}", name, value);
                Save();
            }
        }

        private static string NameOf<T>(Expression<Func<T>> propertyExpression) {
            return PropertySupport.ExtractPropertyName(propertyExpression);
        }

        public void Save() {
            _isolatedStorage.Save();
        }
    }
}