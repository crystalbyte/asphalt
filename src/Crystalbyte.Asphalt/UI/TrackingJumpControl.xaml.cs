#region Using directives

using System;
using System.ComponentModel;
using System.Windows.Input;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Pages;

#endregion

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
            navigation.Service.Navigate(new Uri(string.Format("/Pages/{0}.xaml", typeof (LandingPage).Name),
                                                UriKind.Relative));
        }
    }
}