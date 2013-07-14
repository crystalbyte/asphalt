using System.Linq;
using Crystalbyte.Asphalt.Commands;
using System.Windows.Navigation;

namespace Crystalbyte.Asphalt.Pages {
    public partial class LandingPage {

        // Constructor
        public LandingPage() {
            InitializeComponent();

            DataContext = App.Context;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var navigation = App.Composition.GetExport<Navigation>();
            navigation.Initialize(NavigationService);

            UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }
        }

        private void UpdateApplicationBar() {
            var commands = App.Composition.GetExports<IButtonCommand>()
                .Where(x => x.IsApplicable).OrderBy(x => x.Position);

            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.AddRange(commands.Select(x => x.Button));
        }
    }
}