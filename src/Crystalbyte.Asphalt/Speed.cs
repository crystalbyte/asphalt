﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt {
    public static class Speed {
        public static double GetKmFromMs(double ms) {
            return Math.Round(ms * 3.6, 1);
        }

        public static double GetMsFromKm(double km) {
            return Math.Round(km / 3.6, 1);
        }
    }
}