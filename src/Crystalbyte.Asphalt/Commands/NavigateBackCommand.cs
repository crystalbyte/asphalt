using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Pages;
using Microsoft.Phone.Shell;
using System.Composition;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarButtonCommand))]
    public sealed class NavigateBackCommand : IAppBarButtonCommand {

        [Import]
        public Navigation Navigation { get; set; }

        public NavigateBackCommand() {
            Button = new ApplicationBarIconButton(new Uri("/Assets/ApplicationBar/Back.png", UriKind.Relative)) {
                Text = "back"
            };
            Button.Click += (sender, e) => Execute(null);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get { return false; }
        }

        public int Position {
            get { return 1; }
        }

        public bool CanExecute(object parameter) {
            return Navigation.Service.CanGoBack;
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        public void Execute(object parameter) {
            Navigation.Service.GoBack();
        }
    }
}
