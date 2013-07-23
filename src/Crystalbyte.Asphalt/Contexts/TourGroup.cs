using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt.Contexts {
    /// <summary>
    /// Denotes a coherent list of Tour items.
    /// </summary>
    public sealed class TourGroup : List<Tour> {

        public TourGroup(DateTime month, IEnumerable<Tour> items) : base(items) {
            Month = month;
        }

        public DateTime Month { get; private set; }
    }
}
