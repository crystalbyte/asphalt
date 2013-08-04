using System;
using System.Collections.ObjectModel;
using System.Linq;
using Crystalbyte.Asphalt.Data;
using System.Composition;

namespace Crystalbyte.Asphalt.Contexts {

    [Export, Shared]
    public sealed class AppContext : NotificationObject {

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

        /// <summary>
        /// A collection of recent tours.
        /// </summary>
        public ObservableCollection<Tour> Tours { get; private set; }

        /// <summary>
        /// A collection of recent tours grouped by date.
        /// </summary>
        public object GroupedTours {
            get {
                return Tours
                    .GroupBy(x => new DateTime(x.StartTime.Year, x.StartTime.Month, 1))
                    .Select(x => new TourGroup(x.Key, x)).ToList();
            }
        }

        public bool IsDataLoaded {
            get;
            private set;
        }

        public void InvalidateData() {
            IsDataLoaded = false;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
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