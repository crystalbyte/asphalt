#region Using directives

using System;
using System.Composition;
using System.Data.Linq;
using System.Diagnostics;
using System.Windows;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof (IAppBarMenuCommand))]
    public sealed class DeleteVehicleCommand : IAppBarMenuCommand {
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
            MenuItem = new ApplicationBarMenuItem(AppResources.DeleteButtonText);
            MenuItem.Click += (sender, e) => Execute(null);
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

        #region IAppBarMenuCommand implementation

        public ApplicationBarMenuItem MenuItem { get; private set; }

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

            if (Navigator.Frame.CanGoBack) {
                Navigator.Frame.GoBack();
            }
            else {
                Navigator.Navigate<LandingPage>();
            }
        }

        #endregion
    }
}