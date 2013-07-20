using System.Data.Linq;
using System.Linq;
using System.Windows;
using Crystalbyte.Asphalt.Commands;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Controls;
using Crystalbyte.Asphalt.Data;

namespace Crystalbyte.Asphalt.Pages {
    public partial class LandingPage {
        private bool _isNewPageInstance;

        // Constructor
        public LandingPage() {
            InitializeComponent();
            DataContext = App.Context;

            _isNewPageInstance = true;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (_isNewPageInstance) {
                var navigation = App.Composition.GetExport<Navigation>();
                navigation.Initialize(NavigationService);    
            }

            UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }

            _isNewPageInstance = false;
        }

        private void UpdateApplicationBar() {
            var buttonCommands = App.Composition.GetExports<IButtonCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.AddRange(buttonCommands.Select(x => x.Button));

            var menuCommands = App.Composition.GetExports<IMenuCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            ApplicationBar.MenuItems.Clear();
            ApplicationBar.MenuItems.AddRange(menuCommands.Select(x => x.MenuItem));
        }

        private void OnDeleteMenuItemClicked(object sender, RoutedEventArgs e) {
            var car = (Vehicle) ((MenuItem) sender).DataContext;
            DeleteCarAndReload(car);
        }

        private static void DeleteCarAndReload(Vehicle car) {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            localStorage.CarDataContext.Cars.DeleteOnSubmit(car);
            localStorage.CarDataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);

            if (!string.IsNullOrWhiteSpace(car.ImagePath)) {
                localStorage.DeleteImageAsync(car.ImagePath);
            }

            // Force data reload
            App.Context.LoadData();
        }
    }
}