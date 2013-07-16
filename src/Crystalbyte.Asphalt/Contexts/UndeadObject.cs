using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt.Contexts {
    [DataContract]
    public abstract class UndeadObject : NotificationObject {
        public virtual void OnRevive() {
            // Place tombstoning revival code
        }
    }
}
