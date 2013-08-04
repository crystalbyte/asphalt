using System.Data.Linq;
using System.Diagnostics;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Navigation;

namespace Crystalbyte.Asphalt.Pages {
    public partial class LandingPage {
        private bool _isNewPageInstance;

        // Constructor
        public LandingPage() {
            InitializeComponent();
            DataContext = App.Context;
            Channels = App.Composition.GetExport<Channels>();
            LocalStorage = App.Composition.GetExport<LocalStorage>();

            _isNewPageInstance = true;
        }

        // Import
        public Channels Channels { get; set; }

        // Import
        public LocalStorage LocalStorage { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (_isNewPageInstance) {
                var navigation = App.Composition.GetExport<Navigation>();
                navigation.Initialize(NavigationService);
            }

            this.UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }

            _isNewPageInstance = false;
        }

        private async void OnDeleteTourMenuItemClicked(object sender, RoutedEventArgs e) {
            var item = (MenuItem)sender;
            var tour = (Tour)item.DataContext;

            Debug.WriteLine("Deleting tour with id {0} ...", tour.Id);

            await Channels.Database.Enqueue(() => {
                LocalStorage.DataContext.Tours.DeleteOnSubmit(tour);
                LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
            });

            Debug.WriteLine("Tour with id {0} has been successfully deleted.", tour.Id);

            App.Context.LoadData();
        }
    }
}