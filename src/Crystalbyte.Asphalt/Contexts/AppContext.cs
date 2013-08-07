#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Crystalbyte.Asphalt.Data;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        private bool _isSelectionEnabled;

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        [Import]
        public LocationTracker LocationTracker { get; set; }

        [Import]
        public Channels Channels { get; set; }

        public AppContext() {
            Tours = new ObservableCollection<Tour>();
            Tours.CollectionChanged += (sender, e) => RaisePropertyChanged(() => GroupedTours);
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
        ///   A collection of recent tours.
        /// </summary>
        public ObservableCollection<Tour> Tours { get; private set; }

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

            IsDataLoaded = true;
        }
    }
}