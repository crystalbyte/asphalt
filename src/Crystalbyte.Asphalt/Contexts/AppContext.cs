using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Composition;

namespace Crystalbyte.Asphalt.Contexts {

    [Export, Shared]
    public class AppContext : NotificationObject {

        [Import]
        public LocalStorage LocalStorage { get; set; }

        public AppContext() {
            Cars = new ObservableCollection<Car>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<Car> Cars { get; private set; }
    
        public bool IsDataLoaded {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData() {
            Cars.AddRange(from car in LocalStorage.CarDataContext.Cars select car);
            IsDataLoaded = true;
        }
    }
}