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

        /// <summary>
        /// The LINQ to SQL position table. This property is auto populated.
        /// </summary>
        public Table<Position> Positions;

        /// <summary>
        /// The LINQ to SQL tour table. This property is auto populated.
        /// </summary>
        public Table<Tour> Tours;
    }
}
