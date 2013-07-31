using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Windows.UI.Core;

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class DispatcherService {
        public Dispatcher Dispatcher { get; private set; }

        public void Initialize(Dispatcher dispatcher) {
            Dispatcher = dispatcher;
        }
    }
}
