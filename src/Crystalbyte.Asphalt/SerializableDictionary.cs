using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Crystalbyte.Asphalt {

    [CollectionDataContract]
    public sealed class SerializableDictionary<T> : Dictionary<string, T> { }
}
