using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt.Contexts {
    public abstract class RevisionObject : NotificationObject {
        private readonly Stack<Dictionary<string, object>> _versions =
        new Stack<Dictionary<string, object>>();

        internal void Commit() {
            var values = new Dictionary<string, object>();
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
