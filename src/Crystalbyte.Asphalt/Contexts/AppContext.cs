#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Commands;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        private bool _isSelectionEnabled;
        private bool _isMovementDetectionEnabled;

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        [Import]
        public LocationTracker LocationTracker { get; set; }

        [Import]
        public Channels Channels { get; set; }

        [Import]
        public DeleteTourCommand DeleteTourCommand { get; set; }

        public AppContext() {
            Tours = new ObservableCollection<Tour>();
            Tours.CollectionChanged += (sender, e) => RaisePropertyChanged(() => GroupedTours);

            Vehicles = new ObservableCollection<Vehicle>();
            Drivers = new ObservableCollection<Driver>();
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            LocationTracker.TourStored += (sender, e) => LoadData();
            DeleteTourCommand.DeletionCompleted += OnTourDeletionCompleted;

            // Attach monitorign event handler
            AppSettings.IsMovementDetectionEnabledChanged += (sender, e) => 
                NotifyIsMovementDetectionEnabledChanged();

            // Trigger initial update
            NotifyIsMovementDetectionEnabledChanged();
        }

        private async void OnTourDeletionCompleted(object sender, EventArgs e) {
            await LoadToursAsync();
        }

        public void NotifyIsMovementDetectionEnabledChanged() {
            IsMovementDetectionEnabled = AppSettings.IsMovementDetectionEnabled;
        }

        public bool IsMovementDetectionEnabled {
            get { return _isMovementDetectionEnabled; }
            set {
                if (_isMovementDetectionEnabled == value) {
                    return;
                }

                RaisePropertyChanging(() => IsMovementDetectionEnabled);
                _isMovementDetectionEnabled = value;
                RaisePropertyChanged(() => IsMovementDetectionEnabled);
            }
        }

        public bool IsSelectionEnabled {
            get { return _isSelectionEnabled; }
            set {
                if (_isSelectionEnabled == value) {
                    return;
                }
                RaisePropertyChanging(() => IsSelectionEnabled);
                _isSelectionEnabled = value;
                RaisePropertyChanged(() => IsSelectionEnabled);
                OnSelectionEnabledChanged(EventArgs.Empty);
            }
        }

        public event EventHandler SelectionEnabledChanged;

        public void OnSelectionEnabledChanged(EventArgs e) {
            var handler = SelectionEnabledChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        ///   A collection of all tours.
        /// </summary>
        public ObservableCollection<Tour> Tours { get; private set; }

        /// <summary>
        ///   A collection of all vehicles.
        /// </summary>
        public ObservableCollection<Vehicle> Vehicles { get; private set; }

        /// <summary>
        ///   A collection of all drivers.
        /// </summary>
        public ObservableCollection<Driver> Drivers { get; private set; }

        /// <summary>
        ///   A collection of recent tours grouped by date.
        /// </summary>
        public object GroupedTours {
            get {
                return Tours
                    .GroupBy(x => new DateTime(x.StartTime.Year, x.StartTime.Month, 1))
                    .Select(x => new TourGroup(x.Key, x)).ToList();
            }
        }

        /// <summary>
        ///   Gets whether Tours and Vehicles are loaded.
        /// </summary>
        public bool IsDataLoaded { get; private set; }

        /// <summary>
        ///   Invalidate all Tours and Vehicles are loaded.
        /// </summary>
        public void InvalidateData() {
            IsDataLoaded = false;
        }

        /// <summary>
        ///   Creates and .
        /// </summary>
        public async void LoadData() {
            await LoadToursAsync();
            await LoadVehiclesAsync();
            await LoadDriversAsync();
            IsDataLoaded = true;
        }

        private async Task LoadDriversAsync() {
            Drivers.Clear();
            var tours = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Drivers
                          .Select(x => x)
                          .ToArray());

            Tours.AddRange(tours);
        }

        public async Task LoadToursAsync() {
            Tours.Clear();
            var tours = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Tours
                          .Select(x => x)
                          .OrderByDescending(x => x.StartTime)
                          .ToArray());

            Tours.AddRange(tours);
        }

        public async Task LoadVehiclesAsync() {
            Vehicles.Clear();
            var vehicles = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Vehicles
                          .Select(x => x)
                          .ToArray());

            Vehicles.AddRange(vehicles);
        }
    }
}