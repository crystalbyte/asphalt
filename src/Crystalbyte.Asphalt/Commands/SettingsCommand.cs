#region Using directives

using System;
using System.Composition;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof (IAppBarMenuCommand))]
    public sealed class SettingsCommand : IAppBarMenuCommand {
        public SettingsCommand() {
            MenuItem = new ApplicationBarMenuItem {Text = AppResources.SettingsMenuItemText};
            MenuItem.Click += OnMenuItemClicked;
        }

        private void OnMenuItemClicked(object sender, EventArgs e) {
            Execute(null);
        }

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppSettings AppSettings { get; set; }

        #region IMenuCommand implementation

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            Navigator.Navigate<SettingsPage>();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        public ApplicationBarMenuItem MenuItem { get; private set; }

        public bool IsApplicable {
            get { return Navigator.IsCurrentPageOfType<LandingPage>(); }
        }

        public int Position {
            get { return 1; }
        }

        #endregion
    }
}