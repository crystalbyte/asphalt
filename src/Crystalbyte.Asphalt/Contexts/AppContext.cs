#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Asphalt.Commands;
using Crystalbyte.Asphalt.Data;
using Windows.ApplicationModel.Store;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        private bool _isSelectionEnabled;
        private bool _isMovementDetectionEnabled;
        private SetupState _setupState;
        private bool _suppressEvents;

        [Import]
        public Channels Channels { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        [Import]
        public LocationTracker LocationTracker { get; set; }

        [Import]
        public DeleteTourCommand DeleteTourCommand { get; set; }

        [Import]
        public DeleteVehicleCommand DeleteVehicleCommand { get; set; }

        [Import]
        public DeleteDriverCommand DeleteDriverCommand { get; set; }

        [Import]
        public TourSelectionSource TourSelectionSource { get; set; }

        [Import]
        public VehicleSelectionSource VehicleSelectionSource { get; set; }

        [Import]
        public SaveVehicleCommand SaveVehicleCommand { get; set; }

        [Import]
        public SaveDriverCommand SaveDriverCommand { get; set; }

        [Import]
        public AddVehicleCommand AddVehicleCommand { get; set; }

        [Import]
        public AddDriverCommand AddDriverCommand { get; set; }

        public AppContext() {
            Tours = new ObservableCollection<Tour>();
            Tours.CollectionChanged += (sender, e) => RaisePropertyChanged(() => GroupedTours);

            Vehicles = new ObservableCollection<Vehicle>();
            Drivers = new ObservableCollection<Driver>();
        }

        public event EventHandler SetupStateChanged;

        public void OnSetupStateChanged(EventArgs e) {
            var handler = SetupStateChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler DataUpdated;

        public void OnDataUpdated(EventArgs e) {
            var handler = DataUpdated;
            if (handler != null)
                handler(this, e);
        }

        private void CheckSetupState() {
            var isCompleted = Vehicles.Any(x => x.IsSelected)
                              && Drivers.Any(x => x.IsSelected);

            SetupState = isCompleted ? SetupState.Completed : SetupState.NotCompleted;
        }

        public LicenseInformation License {
            get { return CurrentApp.LicenseInformation; }
        }

        public SetupState SetupState {
            get { return _setupState; }
            private set {
                if (_setupState == value) {
                    return;
                }

                RaisePropertyChanging(() => SetupState);
                _setupState = value;
                RaisePropertyChanged(() => SetupState);
                OnSetupStateChanged(EventArgs.Empty);
            }
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            LocationTracker.TourStored += OnTourStored;
            DeleteTourCommand.DeletionCompleted += OnTourDeletionCompleted;
            DeleteVehicleCommand.DeletionCompleted += OnVehicleDeletionCompleted;
            DeleteDriverCommand.DeletionCompleted += OnDriversDeletionCompleted;
            SaveVehicleCommand.VehicleSaved += OnVehicleSaved;
            SaveDriverCommand.DriverSaved += OnDriverSaved;

            // Attach monitoring event handler
            AppSettings.SettingsChanged += (sender, e) =>
                                           NotifyIsMovementDetectionEnabledChanged();

            // Trigger initial update
            NotifyIsMovementDetectionEnabledChanged();
        }

        private async void OnDriverSaved(object sender, EventArgs e) {
            await LoadDriversAsync();
            RefreshSelections();
            CheckSetupState();
        }

        private async void OnVehicleSaved(object sender, EventArgs e) {
            await LoadVehiclesAsync();
            RefreshSelections();
            CheckSetupState();
        }

        private async void OnDriversDeletionCompleted(object sender, EventArgs e) {
            await LoadDriversAsync();
            RefreshSelections();
            CheckSetupState();
        }

        private async void OnVehicleDeletionCompleted(object sender, EventArgs e) {
            await LoadVehiclesAsync();
            RefreshSelections();
            CheckSetupState();
        }

        private async void OnTourStored(object sender, EventArgs e) {
            await LoadToursAsync();
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

        public void RefreshSelections() {
            Vehicles.ForEach(x => x.InvalidateSelection());
            Drivers.ForEach(x => x.InvalidateSelection());
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
            _suppressEvents = true;
            await LoadVehiclesAsync();
            await LoadDriversAsync();
            await LoadToursAsync();
            _suppressEvents = false;

            RefreshSelections();
            CheckSetupState();
            IsDataLoaded = true;
            OnDataUpdated(EventArgs.Empty);
        }

        private async Task LoadDriversAsync() {
            Drivers.Clear();
            var drivers = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Drivers
                          .Select(x => x)
                          .ToArray());

            Drivers.AddRange(drivers);
            foreach (var driver in drivers) {
                await driver.RestoreImageAsync();
            }
            if (!_suppressEvents) {
                OnDataUpdated(EventArgs.Empty);
            }
        }

        public async Task LoadToursAsync() {
            Tours.Clear();
            var tours = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Tours
                          .Select(x => x)
                          .OrderByDescending(x => x.StartTime)
                          .ToArray());

            Tours.AddRange(tours);
            if (!_suppressEvents) {
                OnDataUpdated(EventArgs.Empty);
            }
        }

        public async Task LoadVehiclesAsync() {
            Vehicles.Clear();
            var vehicles = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Vehicles
                          .Select(x => x)
                          .ToArray());

            Vehicles.AddRange(vehicles);
            foreach (var vehicle in vehicles) {
                await vehicle.RestoreImageAsync();
            }
            if (!_suppressEvents) {
                OnDataUpdated(EventArgs.Empty);
            }
        }
    }
}