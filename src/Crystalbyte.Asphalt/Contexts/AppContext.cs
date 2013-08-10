#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
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
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            LocationTracker.TourStored += (sender, e) => LoadData();
            DeleteTourCommand.DeletionCompleted += (sender, e) => LoadData();
            AppSettings.IsMovementDetectionEnabledChanged += (sender, e) => 
                NotifyIsMovementDetectionEnabledChanged();
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
        ///   A collection of recent tours grouped by date.
        /// </summary>
        public object GroupedTours {
            get {
                return Tours
                    .GroupBy(x => new DateTime(x.StartTime.Year, x.StartTime.Month, 1))
                    .Select(x => new TourGroup(x.Key, x)).ToList();
            }
        }

        public bool IsDataLoaded { get; private set; }

        public void InvalidateData() {
            IsDataLoaded = false;
        }

        /// <summary>
        ///   Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public async void LoadData() {
            Tours.Clear();
            var tours = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Tours
                          .Select(x => x)
                          .OrderByDescending(x => x.StartTime)
                          .ToArray());

            Tours.AddRange(tours);

            Vehicles.Clear();
            var vehicles = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Vehicles
                          .Select(x => x)
                          .ToArray());

            Vehicles.AddRange(vehicles);

            IsDataLoaded = true;
        }
    }
}