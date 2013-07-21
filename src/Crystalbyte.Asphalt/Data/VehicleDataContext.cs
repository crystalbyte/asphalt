using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Data {
    public sealed class VehicleDataContext : DataContext {
        public VehicleDataContext(string connectionString) 
            : base(connectionString) {
        }

        public Table<Vehicle> Vehicles;
    }
}
