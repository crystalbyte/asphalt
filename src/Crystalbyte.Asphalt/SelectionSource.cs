#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt {
    public abstract class SelectionSource<T> : NotificationObject where T : class {
        public event EventHandler SelectionChanged;

        protected SelectionSource() {
            Selections = new ObservableCollection<T>();
            Selections.CollectionChanged += OnSelectionsCollectionChanged;
        }

        private void OnSelectionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnSelectionChanged(EventArgs.Empty);
        }

        public void OnSelectionChanged(EventArgs e) {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public T Selection {
            get { return Selections.FirstOrDefault(); }
            set {
                if (Selection == value && Selections.Count == 1) {
                    return;
                }

                RaisePropertyChanging(() => Selection);
                Selections.Clear();
                Selections.Add(value);
                RaisePropertyChanged(() => Selection);
            }
        }

        public ObservableCollection<T> Selections { get; private set; }
    }
}