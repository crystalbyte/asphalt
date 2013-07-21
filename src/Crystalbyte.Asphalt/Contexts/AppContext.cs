﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
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

        public AppContext() {
            Vehicles = new ObservableCollection<Vehicle>();
            History = new ObservableCollection<Tour>();
            History.CollectionChanged += (sender, e) => RaisePropertyChanged(() => GroupedHistory);
        }

        /// <summary>
        /// A collection of all vehicles.
        /// </summary>
        public ObservableCollection<Vehicle> Vehicles { get; private set; }

        /// <summary>
        /// A collection of recent tours.
        /// </summary>
        public ObservableCollection<Tour> History { get; private set; }

        /// <summary>
        /// A collection of recent tours grouped by date.
        /// </summary>
        public object GroupedHistory {
            get { return History.GroupBy(x => x.StartTime); }
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
            Vehicles.Clear();
            Vehicles.AddRange(LocalStorage.VehicleDataContext.Vehicles.Select(x => x));
            IsDataLoaded = true;
        }
    }
}