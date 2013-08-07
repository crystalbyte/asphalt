#region Using directives

using System.Composition;

#endregion

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class Channels {
        public Channels() {
            Database = new ConcurrentQueue(1);
        }

        public ConcurrentQueue Database { get; private set; }
    }
}