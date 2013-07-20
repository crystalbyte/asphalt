using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Data {
    public sealed class CarDataContext : DataContext {
        public CarDataContext(string connectionString) 
            : base(connectionString) {
        }

        public Table<Vehicle> Cars;
    }
}
