﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Pages;
using Microsoft.Phone.Shell;
using System.Composition;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Resources;
using Crystalbyte.Asphalt.Data;

namespace Crystalbyte.Asphalt.Commands {

    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class SaveDriverCommand : IAppBarButtonCommand {

        public SaveDriverCommand() {
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

            if (Navigator.Service.CanGoBack) {
                Navigator.Service.GoBack();
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

        public ApplicationBarIconButton Button {
            get;
            private set;
        }

        public bool IsApplicable {
            get {
                var page = Navigator.GetCurrentPage<DriverCompositionPage>();
                return page != null;
            }
        }

        public int Position {
            get { return 0; }
        }

        #endregion
    }
}