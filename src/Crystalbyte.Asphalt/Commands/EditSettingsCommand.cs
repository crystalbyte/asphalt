using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Asphalt.Commands;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Shell;
using System.Composition;
using Crystalbyte.Asphalt.Resources;
using Crystalbyte.Asphalt.Pages;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class EditSettingsCommand : IAppBarButtonCommand {

        public EditSettingsCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Edit.png", UriKind.Relative)) {
                Text = AppResources.EditSettingsButtonText
            };
            Button.Click += (sender, e) => Execute(null);
        }

        [Import]
        public AppSettings AppSettings { get; set; }

        [Import]
        public Navigator Navigator { get; set; }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IsApplicable;
        }

        public void Execute(object parameter) {
            AppSettings.IsEditing = true;
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
                var page = Navigator.GetCurrentPage<SettingsPage>();
                if (page != null) {
                    return !AppSettings.IsEditing;
                }

                return false;
            }
        }

        public int Position {
            get { return 0; }
        }

        #endregion
    }
}
