﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt {
    public static class Speed {
        public static double GetKmFromMs(double ms) {
            return ms * 3.6;
        }
    }
}
