#region Using directives

using System.ComponentModel;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.UI;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class ExportPage {
        public ExportPage() {
            InitializeComponent();

            if (DesignerProperties.IsInDesignTool) {
                return;
            }
            
            ExportContext = App.Composition.GetExport<ExportContext>();
            BackgroundImageSource.ImageSource = ThemedResourceProvider.SatellitePageBackgroundSource;
        }

        public ThemedResourceProvider ThemedResourceProvider {
            get { return App.Composition.GetExport<ThemedResourceProvider>(); }
        }

        public ExportContext ExportContext {
            get { return DataContext as ExportContext; }
            set { DataContext = value; }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            await ExportContext.RefreshAsync();

            FormatPicker.SelectedIndex = 0;
            StrategyPicker.SelectedIndex = 0;
        }
    }
}