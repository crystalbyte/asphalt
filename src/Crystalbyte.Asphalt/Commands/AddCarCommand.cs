using System;
using System.Collections.Generic;
using System.Composition;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Shell;
using System.Windows.Controls;

namespace Crystalbyte.Asphalt.Commands {
    [Export(typeof(AddCarCommand))]
    [Export(typeof(IButtonCommand)), Shared]
    public sealed class AddCarCommand : IButtonCommand {

        [Import]
        public Navigation Navigation { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        public AddCarCommand() {
            Button = new ApplicationBarIconButton(new Uri(@"Assets/ApplicationBar/Add.png", UriKind.Relative)) {
                Text = AppResources.AddCarCommandText,
            };
            Button.Click += OnButtonClicked;
        }

        private void OnButtonClicked(object sender, EventArgs e) {
            Execute(this);
        }

        public ApplicationBarIconButton Button { get; private set; }

        public bool IsApplicable {
            get {
                return ((Frame)Application.Current.RootVisual).Content.GetType() == typeof(LandingPage);
            }
        }

        public int Position { get { return 0; } }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        public void Execute(object parameter) {
            var car = new Car();

            NavigationState.Push(LocalStorage);
            NavigationState.Push(car);
            Navigation.Service.Navigate(new Uri(string.Format("/Pages/{0}.xaml?action=new", typeof(CarCompositionPage).Name), UriKind.Relative));
        }
    }
}
