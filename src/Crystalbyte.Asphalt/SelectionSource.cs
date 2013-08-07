#region Using directives

using System;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt {
    public abstract class SelectionSource<T> : NotificationObject where T : class {
        public event EventHandler SelectionChanged;

        public void OnSelectionChanged(EventArgs e) {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        private T _selection;

        public T Selection {
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