using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt.Contexts {
    [DataContract]
    public abstract class RevisionObject : UndeadObject {
        private Stack<SerializableDictionary<object>> _versions =
        new Stack<SerializableDictionary<object>>();

        [DataMember]
        public List<SerializableDictionary<object>> Versions {
            get {
                return new List<SerializableDictionary<object>>(_versions);
            }
            set {
                _versions = new Stack<SerializableDictionary<object>>(value);
            }
        }

        public override void OnRevive() {
            base.OnRevive();
            if (_versions == null) {
                _versions = new Stack<SerializableDictionary<object>>();
            }
        }

        internal void Commit() {
            var values = new SerializableDictionary<object>();
            var properties = GetType().GetProperties();
            properties.ForEach(x => {
                if (x.CustomAttributes.Any(a => a.AttributeType == typeof(MemorizeAttribute))) {
                    values.Add(x.Name, x.GetValue(this));
                }
            });
            _versions.Push(values);
        }

        internal void Revert() {
            var values = _versions.Pop();
            var type = GetType();
            values.ForEach(x => {
                var property = type.GetProperty(x.Key);
                try {
                    property.SetValue(this, x.Value);
                }
                catch (TargetInvocationException ex) {
                    Debug.WriteLine(ex.ToString());
                }
                catch (ValidationException ex) {
                    Debug.WriteLine(ex.ToString());
                }
            });
        }

        public bool CanRevert { get { return _versions.Count > 0; } }
    }
}
