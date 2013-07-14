using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt {
    public enum TourType {
        /// <summary>
        /// Business denotes a work related trip apart from the commute.
        /// </summary>
        Business = 0,
        /// <summary>
        /// Private denotes a non work releated trip.
        /// </summary>
        Private,
        /// <summary>
        /// Commute denotes the trip from home to work and vice versa.
        /// </summary>
        Commute
    }
}
