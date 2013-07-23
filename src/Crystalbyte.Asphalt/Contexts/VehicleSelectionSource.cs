using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class VehicleSelectionSource : NotificationObject {

        public event EventHandler SelectionChanged;

        public void OnSelectionChanged(EventArgs e) {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        private Vehicle _selection;
        public Vehicle Selection {
            get { return _selection; }
            set {
                if (_selection == value) {
                    return;
                }

                RaisePropertyChanging(() => Selection);
                _selection = value;
                RaisePropertyChanged(() => Selection);
                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }
}
