#region Using directives

using System.Composition;
using System.Windows.Navigation;

#endregion

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class Navigation {
        public NavigationService Service { get; private set; }

        public void Initialize(NavigationService service) {
            Service = service;
        }
    }
}