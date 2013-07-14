using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Phone.Shell;

namespace Crystalbyte.Asphalt.Commands {
    public interface IButtonCommand : ICommand {
        ApplicationBarIconButton Button { get; }
        bool IsApplicable { get; }
        int Position { get; }
    }
}
