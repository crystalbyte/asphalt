#region Using directives

using System;
using System.Composition;
using System.Linq;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof (IAppBarButtonCommand))]
    public sealed class DeleteSelectedToursCommand : IAppBarButtonCommand {
        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public TourSelectionSource TourSelectionSource { get; set; }

        [Import]
        public Channels Channels { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public DeleteTourCommand DeleteTourCommand { get; set; }

        public DeleteSelectedToursCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Delete.png", UriKind.Relative))
                         {
                             Text = AppResources.DeleteButtonText
                         };
            Button.Click += (sender, e) => Execute(null);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IsApplicable;
        }

        public void Execute(object parameter) {
            DeleteTourCommand.Execute(parameter);
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
                // Display on Landing page only with an active selection.
                var landingPage = Navigator.GetCurrentPage<LandingPage>();
                if (landingPage != null) {
                    return TourSelectionSource.Selections.Any() && landingPage.PanoramaIndex == 1;
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