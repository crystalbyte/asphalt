#region Using directives

using System;
using System.Composition;
using System.Linq;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof (IAppBarButtonCommand))]
    public sealed class EditDriverCommand : IAppBarButtonCommand {
        public EditDriverCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Edit.png", UriKind.Relative))
                         {
                             Text = AppResources.EditVehicleButtonText
                         };
            Button.Click += (sender, e) => Execute(null);
        }

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public DriverSelectionSource DriverSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            AppContext.Drivers.CollectionChanged +=
                (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IsApplicable;
        }

        public void Execute(object parameter) {
            DriverSelectionSource.Selection = AppContext.Drivers.First(x => x.IsSelected);
            Navigator.Navigate<DriverCompositionPage>();
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
                var page = Navigator.GetCurrentPage<LandingPage>();
                if (page != null) {
                    return page.PanoramaIndex == 3
                           && AppContext.Drivers.Any(x => x.IsSelected);
                }

                return false;
            }
        }

        public int Position {
            get { return 2; }
        }

        #endregion
    }
}