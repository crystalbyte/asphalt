using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Pages;
using Microsoft.Phone.Shell;
using Crystalbyte.Asphalt.Resources;
using System.Composition;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class DeleteVehicleCommand : IAppBarButtonCommand {

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public VehicleSelectionSource VehicleSelectionSource { get; set; }

        [Import]
        public Channels Channels { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public LocationTracker LocationTracker { get; set; }

        public DeleteVehicleCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Delete.png", UriKind.Relative)) {
                Text = AppResources.DeleteButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        public event EventHandler DeletionCompleted;

        public void OnDeletionCompleted(EventArgs e) {
            var handler = DeletionCompleted;
            if (handler != null)
                handler(this, e);
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            VehicleSelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #region IAppBarButtonCommand implementation

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                // Don't display delete button if there is nothing to delete.
                if (AppContext.IsDataLoaded && AppContext.Vehicles.Count == 0) {
                    return false;
                }

                // Display always on details page.
                var page = Navigator.GetCurrentPage<VehicleCompositionPage>();
                return page != null;
            }
        }

        public int Position {
            get { return 2; }
        }

        public bool CanExecute(object parameter) {
            return IsApplicable && !LocationTracker.IsTracking;
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        public async void Execute(object parameter) {
            var caption = AppResources.DeleteVehicleConfirmCaption;
            var message = AppResources.DeleteVehicleConfirmMessage;
            var result = MessageBox.Show(message, caption, MessageBoxButton.OKCancel);
            if (result.HasFlag(MessageBoxResult.Cancel)) {
                Debug.WriteLine("Deletion aborted by user.");
                return;
            }

            Debug.WriteLine("Deleting selected vehicle ...");

            var vehicle = VehicleSelectionSource.Selection;

            await Channels.Database.Enqueue(() => {
                LocalStorage.DataContext.Vehicles.DeleteOnSubmit(vehicle);
                LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
            });

            OnDeletionCompleted(EventArgs.Empty);

            Debug.WriteLine("Selected vehicle has been successfully deleted.");

            var page = Navigator.GetCurrentPage<VehicleCompositionPage>();
            if (page == null) 
                return;

            if (Navigator.Service.CanGoBack) {
                Navigator.Service.GoBack();
            } else {
                Navigator.Navigate<LandingPage>();
            }
        }

        #endregion
    }
}
