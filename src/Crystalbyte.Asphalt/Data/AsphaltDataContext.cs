#region Using directives

using System.Data.Linq;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt.Data {
    public sealed class AsphaltDataContext : DataContext {
        public AsphaltDataContext(string connectionString)
            : base(connectionString) {}

        /// <summary>
        ///   The LINQ to SQL position table. This property is auto populated.
        /// </summary>
        public Table<Position> Positions;

        /// <summary>
        ///   The LINQ to SQL tour table. This property is auto populated.
        /// </summary>
        public Table<Tour> Tours;
    }
}