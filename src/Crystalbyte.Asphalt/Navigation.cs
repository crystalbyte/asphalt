using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class Navigation
    {
        public NavigationService Service { get; private set; }
        public void Initialize(NavigationService service) {
            Service = service;
        }
    }
}
