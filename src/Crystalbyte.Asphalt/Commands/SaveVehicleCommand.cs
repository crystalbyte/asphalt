#region Using directives

using System;
using System.Composition;
using System.Data.Linq;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class SaveVehicleCommand : IAppBarButtonCommand {
        public SaveVehicleCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Save.png", UriKind.Relative)) {
                Text = AppResources.SaveVehicleButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public VehicleSelectionSource VehicleSelectionSource { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public Channels Channels { get; set; }

        public event EventHandler VehicleSaved;

        public void OnVehicleSaved(EventArgs e) {
            var handler = VehicleSaved;
            if (handler != null)
                handler(this, e);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IsApplicable;
        }

        public async void Execute(object parameter) {
            var vehicle = VehicleSelectionSource.Selection;

            await Channels.Database.Enqueue(() => {
                var context = LocalStorage.DataContext;
                try {
                    if (vehicle.IsNew) {
                        context.Vehicles.InsertOnSubmit(vehicle);
                    }
                    context.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                catch (ChangeConflictException) {
                    context.ChangeConflicts.ResolveAll(RefreshMode.KeepChanges);
                }
            });

            OnVehicleSaved(EventArgs.Empty);

            if (Navigator.Frame.CanGoBack) {
                Navigator.Frame.GoBack();
            } else {
                Navigator.Navigate<LandingPage>();
            }
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Implementation of IAppBarButtonCommand

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                var vehicle = VehicleSelectionSource.Selection;
                var page = Navigator.GetCurrentPage<VehicleCompositionPage>();
                return page != null && vehicle.IsNew;
            }
        }

        public int Position {
            get { return 0; }
        }

        #endregion
    }
}