using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Crystalbyte.Asphalt {
    public partial class MainPage : PhoneApplicationPage {
        // Constructor
        public MainPage() {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.AppContext;
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (!App.AppContext.IsDataLoaded) {
                App.AppContext.LoadData();
            }
        }
    }
}