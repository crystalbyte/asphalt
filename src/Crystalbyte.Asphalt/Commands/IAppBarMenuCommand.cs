#region Using directives

using System.Windows.Input;
using Microsoft.Phone.Shell;

#endregion

namespace Crystalbyte.Asphalt.Commands {
    public interface IAppBarMenuCommand : ICommand {
        ApplicationBarMenuItem MenuItem { get; }
        bool IsApplicable { get; }
        int Position { get; }
    }
}