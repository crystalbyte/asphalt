using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Crystalbyte.Asphalt.Resources;
using System.Composition;

namespace Crystalbyte.Asphalt.Contexts {

    [Export, Shared]
    public class AppContext : NotificationObject {

        public AppContext() {
            Cars = new ObservableCollection<CarContext>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<CarContext> Cars { get; private set; }
    
        public bool IsDataLoaded {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData() {
            // Sample data; replace with real data
      

            IsDataLoaded = true;
        }
    }
}