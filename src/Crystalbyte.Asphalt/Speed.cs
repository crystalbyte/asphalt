#region Using directives

using System;

#endregion

namespace Crystalbyte.Asphalt {
    public static class Speed {
        public static double GetKmFromMs(double ms) {
            return Math.Round(ms*3.6, 1);
        }

        public static double GetMsFromKm(double km) {
            return Math.Round(km/3.6, 1);
        }
    }
}