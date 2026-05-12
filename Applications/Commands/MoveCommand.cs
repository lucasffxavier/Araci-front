using Araci.ViewModels;
using System.Windows;

namespace Araci.Applications.Commands
{
    public class MoveCommand : ICommandHandler
    {
        public void Execute(ElementoViewModel vm, Vector? delta = null)
        {
            if (delta == null)
                return;

            vm.X += delta.Value.X;
            vm.Y += delta.Value.Y;

            if (vm is CaboViewModel cabo)
            {
                cabo.X2 += delta.Value.X;
                cabo.Y2 += delta.Value.Y;
            }
        }
    }
}