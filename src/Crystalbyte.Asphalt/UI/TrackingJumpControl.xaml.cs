using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Crystalbyte.Asphalt.Pages;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.UI {
    public partial class TrackingJumpControl {
        public TrackingJumpControl() {
            InitializeComponent();
            if (!DesignerProperties.IsInDesignTool) {
                DataContext = App.Composition.GetExport<LocationTracker>();    
            }
        }

        private void OnOverlayRectangleTapped(object sender, GestureEventArgs e) {
            var navigation = App.Composition.GetExport<Navigation>();
            navigation.Service.Navigate(new Uri(string.Format("/Pages/{0}.xaml", typeof(LocationTrackingPage).Name), UriKind.Relative));
        }
    }
}
