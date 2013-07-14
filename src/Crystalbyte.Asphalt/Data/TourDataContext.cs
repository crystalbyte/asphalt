using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt.Data {
    public sealed class TourDataContext : DataContext {
        public TourDataContext(string connectionString) 
            : base(connectionString) {
        }

        
    }
}
