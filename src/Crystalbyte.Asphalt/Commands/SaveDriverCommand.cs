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
    [Export(typeof (IAppBarButtonCommand))]
    public sealed class SaveDriverCommand : IAppBarButtonCommand {
        public SaveDriverCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Save.png", UriKind.Relative))
                         {
                             Text = AppResources.SaveVehicleButtonText
                         };
            Button.Click += (sender, e) => Execute(null);
        }

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public DriverSelectionSource DriverSelectionSource { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public Channels Channels { get; set; }

        public event EventHandler DriverSaved;

        public void OnDriverSaved(EventArgs e) {
            var handler = DriverSaved;
            if (handler != null)
                handler(this, e);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IsApplicable;
        }

        public async void Execute(object parameter) {
            var driver = DriverSelectionSource.Selection;
            var id = driver.Id;

            await Channels.Database.Enqueue(() => {
                                                if (id == 0) {
                                                    LocalStorage.DataContext.Drivers.InsertOnSubmit(driver);
                                                }
                                                LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
                                            });

            OnDriverSaved(EventArgs.Empty);

            if (Navigator.Frame.CanGoBack) {
                Navigator.Frame.GoBack();
            }
            else {
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
                var driver = DriverSelectionSource.Selection;
                var page = Navigator.GetCurrentPage<DriverCompositionPage>();
                return page != null && driver.IsNew;
            }
        }

        public int Position {
            get { return 0; }
        }

        #endregion
    }
}