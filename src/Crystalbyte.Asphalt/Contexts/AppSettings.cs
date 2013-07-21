using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt.Contexts {

    [Export]
    public sealed class AppSettings : NotificationObject {

        private readonly IsolatedStorageSettings _isolatedStorage;

        public AppSettings() {
            _isolatedStorage = IsolatedStorageSettings.ApplicationSettings;
        }

        public event EventHandler BackgroundServiceChanged;

        public void OnBackgroundServiceChanged(EventArgs e) {
            var handler = BackgroundServiceChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Returns true if background services are enabled, false otherwise.
        /// </summary>
        public bool IsBackgroundServiceEnabled {
            get {
                var name = NameOf(() => IsBackgroundServiceEnabled);
                if (_isolatedStorage.Contains(name)) {
                    return (bool)_isolatedStorage[name];
                }
                return false;
            }
            set {
                if (IsBackgroundServiceEnabled == value) {
                    return;
                }

                var name = NameOf(() => IsBackgroundServiceEnabled);
                if (!_isolatedStorage.Contains(name)) {
                    _isolatedStorage.Add(name, false);
                }
                RaisePropertyChanging(() => IsBackgroundServiceEnabled);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => IsBackgroundServiceEnabled);

                OnBackgroundServiceChanged(EventArgs.Empty);

                Save();
            }
        }

        /// <summary>
        /// The unit of length used for export and displaying data.
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
                    _isolatedStorage.Add(name, false);
                }

                RaisePropertyChanging(() => UnitOfLength);
                _isolatedStorage[name] = value;
                RaisePropertyChanged(() => UnitOfLength);

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
