#region Using directives

using System;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    public sealed class RelayCommand : ICommand {
        public Func<object, bool> CanExecuteCallback { get; set; }
        public Action<object> ExecuteCallback { get; set; }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return CanExecuteCallback == null || CanExecuteCallback(parameter);
        }

        public void Execute(object parameter) {
            if (ExecuteCallback != null) {
                ExecuteCallback(parameter);
            }
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}