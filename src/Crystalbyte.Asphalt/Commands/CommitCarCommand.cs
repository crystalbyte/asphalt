using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Commands {
    [Export, Shared]
    public sealed class CommitCarCommand : ICommand {

        [Import]
        public AppContext AppContext { get; set; }

        #region ICommand implementation

        public bool CanExecute(object parameter) {
            
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
