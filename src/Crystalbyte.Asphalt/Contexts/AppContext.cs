using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Composition;
using Crystalbyte.Asphalt.Commands;

namespace Crystalbyte.Asphalt.Contexts {

    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        [Import]
        public LocationTracker LocationTracker { get; set; }

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
            get { return Tours.GroupBy(x => x.StartTime); }
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
        public void LoadData() {
            Tours.Clear();
            Tours.AddRange(LocalStorage.DataContext.Tours
                .Select(x => x).OrderByDescending(x => x.StartTime));

            IsDataLoaded = true;
        }
    }
}