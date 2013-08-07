#region Using directives

using System;
using System.Windows;
using System.Windows.Threading;

#endregion

namespace Crystalbyte.Asphalt {
    public static class SmartDispatcher {
        private static readonly Dispatcher CurrentDispatcher;

        static SmartDispatcher() {
            CurrentDispatcher = Deployment.Current.Dispatcher;
        }

        public static void InvokeAsync(Action action) {
            if (!CurrentDispatcher.CheckAccess()) {
                CurrentDispatcher.BeginInvoke(action);
            }
            else {
                action();
            }
        }
    }
}