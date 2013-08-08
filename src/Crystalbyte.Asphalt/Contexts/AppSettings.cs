#region Using directives

using System;
using System.Composition;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq.Expressions;

#endregion

namespace Crystalbyte.Asphalt.Contexts {

    [Export]
    public sealed class AppSettings : NotificationObject {
        private readonly IsolatedStorageSettings _isolatedStorage;

        [Import]
        public LocationTracker LocationTracker { get; set; }

        public AppSettings() {
            _isolatedStorage = IsolatedStorageSettings.ApplicationSettings;
        }

        public event EventHandler IsMovementDetectionEnabledChanged;

        public void OnIsMovementDetectionEnabledChanged(EventArgs e) {
            var handler = IsMovementDetectionEnabledChanged;
            if (handler != null)
                handler(this, e);

            if (IsMovementDetectionEnabled && !App.IsGeolocatorAlive) {
                App.InitializeGeolocator();
            } else {
                // Don't interrupt active recording.
                if (!LocationTracker.IsTracking) {
                    App.TombstoneGeolocator();    
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the app should automatically start and stop recordings based on its heuristics.
        /// </summary>
        public bool IsMovementDetectionEnabled {
            get {
                var name = NameOf(() => IsMovementDetectionEnabled);
                if (_isolatedStorage.Contains(name)) {
                    return (bool)_isolatedStorage[name];
                }
                return false;
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
                OnIsMovementDetectionEnabledChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        ///   The unit of length used for export and displaying data.
        /// </summary>
        public UnitOfLength UnitOfLength {
            get {
                var name = NameOf(() => UnitOfLength);
                if (_isolatedStorage.Contains(name)) {
                    return (UnitOfLength)_isolatedStorage[name];
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