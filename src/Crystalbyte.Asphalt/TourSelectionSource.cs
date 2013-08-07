#region Using directives

using System.Composition;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class TourSelectionSource : SelectionSource<Tour> {}
}