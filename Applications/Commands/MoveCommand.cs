using Araci.ViewModels;
using System.Windows;

namespace Araci.Applications.Commands
{
    public class MoveCommand : ICommandHandler
    {
        public void Execute(
            ElementoViewModel vm,
            Vector? delta = null)
        {
            if (delta == null)
                return;

            vm.Mover(delta.Value);
        }
    }
}