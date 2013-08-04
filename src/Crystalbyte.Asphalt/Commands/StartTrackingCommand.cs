using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    public sealed class StartTrackingCommand : ICommand {

        [Import]
        public LocationTracker LocationTracker { get; set; }

        public bool CanExecute(object parameter) {
            return !LocationTracker.IsTracking;
        }

        public void Execute(object parameter) {
            LocationTracker.StartTrackingManually();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            LocationTracker.IsTrackingChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }
    }
}
