using Crystalbyte.Asphalt.Resources;
using System.Composition;

namespace Crystalbyte.Asphalt {
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    [Export]
    public sealed class LocalizedStrings {
        private static readonly AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}