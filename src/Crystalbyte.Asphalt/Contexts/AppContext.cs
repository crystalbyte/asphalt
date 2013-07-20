using System;
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

        public AppContext() {
            Cars = new ObservableCollection<Vehicle>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<Vehicle> Cars { get; private set; }
    
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
            Cars.Clear();
            Cars.AddRange(LocalStorage.CarDataContext.Cars.Select(x => x));
            IsDataLoaded = true;
        }
    }
}