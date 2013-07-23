using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Data {
    public sealed class AsphaltDataContext : DataContext {
        public AsphaltDataContext(string connectionString) 
            : base(connectionString) {
        }

        public Table<Position> Positions;
        public Table<Vehicle> Vehicles;
        public Table<Tour> Tours;
    }
}
