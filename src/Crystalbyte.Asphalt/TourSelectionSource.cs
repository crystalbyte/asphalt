using System;
using System.Composition;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt {

    [Export, Shared]
    public sealed class TourSelectionSource : SelectionSource<Tour> { }
}
