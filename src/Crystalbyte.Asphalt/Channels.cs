using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class Channels {

        public Channels() {
            Database = new ConcurrentQueue(1);
        }

        public ConcurrentQueue Database { get; private set; }
    }
}
