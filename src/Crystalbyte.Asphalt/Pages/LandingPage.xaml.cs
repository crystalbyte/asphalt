using System;
using System.Data.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Commands;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Controls;
using Crystalbyte.Asphalt.Data;
using Windows.UI.Core;

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

                var dispatcher = App.Composition.GetExport<DispatcherService>();
                dispatcher.Initialize(Dispatcher);
            }

            this.UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }

            _isNewPageInstance = false;
        }
    }
}