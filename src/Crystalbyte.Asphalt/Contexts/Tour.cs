using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Crystalbyte.Asphalt.Contexts {
    public sealed class Tour : BindingModelBase<Tour> {

        public Tour() {
            Positions = new List<Geoposition>();
        }

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        public List<Geoposition> Positions { get; private set; }

        public string Reason { get; set; }
        public string Destination { get; set; }
    }
}
