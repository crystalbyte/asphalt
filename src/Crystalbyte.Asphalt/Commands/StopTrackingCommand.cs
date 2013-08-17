#region Using directives

using System;
using System.Composition;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Resources;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    public sealed class StopTrackingCommand : ICommand {
        [Import]
        public LocationTracker LocationTracker { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        public bool CanExecute(object parameter) {
            return LocationTracker.IsTracking;
        }

        public void Execute(object parameter) {
            var message = AppResources.TerminateRouteConfirmMessage;
            var caption = AppResources.TerminateRouteConfirmCaption;
            var result = MessageBox.Show(message, caption, MessageBoxButton.OKCancel);

            if (result.HasFlag(MessageBoxResult.Cancel)) {
                return;
            }

            LocationTracker.StopTrackingManually();
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