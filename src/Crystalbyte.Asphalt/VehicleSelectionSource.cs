using Crystalbyte.Asphalt.Contexts;
using System.Composition;

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class VehicleSelectionSource : SelectionSource<Vehicle> { }
}
