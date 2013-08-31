using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystalbyte.Asphalt.UI {
    [Export, Shared]
    public sealed class ThemedResourceProvider {
        private static Uri SelectBackgroundImage(string dark, string light) {
            var isDark = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;
            return isDark ? new Uri(dark, UriKind.Relative) : new Uri(light, UriKind.Relative);
        }

        public ImageSource LandingPageBackgroundSource {
            get { return new BitmapImage(SelectBackgroundImage("/Assets/BackgroundDark.jpg", "/Assets/BackgroundLight.jpg")); }
        }

        public ImageSource SatellitePageBackgroundSource {
            get { return new BitmapImage(SelectBackgroundImage("/Assets/VerticalBackgroundDark.jpg", "/Assets/VerticalBackgroundLight.jpg")); }
        }
    }
}
