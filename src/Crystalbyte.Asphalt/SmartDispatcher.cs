using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Crystalbyte.Asphalt {
    public static class SmartDispatcher {
        private static readonly Dispatcher CurrentDispatcher;

        static SmartDispatcher() {
            CurrentDispatcher = Deployment.Current.Dispatcher;
        }

        public static void InvokeAsync(Action action) {
            if (!CurrentDispatcher.CheckAccess()) {
                CurrentDispatcher.BeginInvoke(action);
            } else {
                action();
            }
        }

    }
}
