#region Using directives

using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace Crystalbyte.Asphalt {
    [CollectionDataContract]
    public sealed class SerializableDictionary<T> : Dictionary<string, T> {}
}