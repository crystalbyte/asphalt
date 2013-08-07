#region Using directives

using System;
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    /// <summary>
    ///   Denotes a coherent list of Tour items.
    /// </summary>
    public sealed class TourGroup : List<Tour> {
        public TourGroup(DateTime month, IEnumerable<Tour> items) : base(items) {
            Month = month;
        }

        public DateTime Month { get; private set; }
    }
}