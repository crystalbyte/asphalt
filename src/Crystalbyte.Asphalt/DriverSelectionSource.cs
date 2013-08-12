using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Contexts;
using System.Composition;

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class DriverSelectionSource : SelectionSource<Driver> { }
}
