#region Using directives

using System;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Pages;
using Microsoft.Phone.Controls;
using System.Composition;
using System.Windows.Navigation;

#endregion

namespace Crystalbyte.Asphalt {
    [Export, Shared]
    public sealed class Navigator {

        public PhoneApplicationFrame Frame {
            get { return Application.Current.RootVisual as PhoneApplicationFrame; }
        }

        public bool IsCurrentPageOfType<T>() {
            return ((Frame)Application.Current.RootVisual).Content.GetType() == typeof(T);
        }

        public void Navigate<T>(string query = "") where T : PhoneApplicationPage {
            Frame.Navigate(new Uri(string.Format("/Pages/{0}.xaml?param={1}", typeof(T).Name, query),
                                                UriKind.Relative));
        }

        public T GetCurrentPage<T>() where T : PhoneApplicationPage {
            return ((Frame) Application.Current.RootVisual).Content as T;
        }
    }
}