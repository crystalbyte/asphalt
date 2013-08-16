using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using System.Composition;
using Crystalbyte.Asphalt.Resources;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarMenuCommand))]
    public sealed class ResetSettingsCommand : IAppBarMenuCommand {

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        public ResetSettingsCommand() {
            MenuItem = new ApplicationBarMenuItem(AppResources.ResetSettingsMenuText);
            MenuItem.Click += (sender, e) => Execute(null);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            AppSettings.ReportInterval = 2000;
            AppSettings.SpeedThreshold = 7.22;
            AppSettings.RecordingTimeout = 3.5;
            AppSettings.RequiredAccuracy = 55;
            AppSettings.SpeedExceedances = 2;
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Implementation of IAppBarMenuCommand

        public ApplicationBarMenuItem MenuItem {
            get; private set;
        }

        public bool IsApplicable {
            get {
                var page = Navigator.GetCurrentPage<SettingsPage>();
                return page != null;
            }
        }

        public int Position {
            get { return 0; }
        }

        #endregion
    }
}
