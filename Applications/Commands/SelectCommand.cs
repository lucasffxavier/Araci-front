using Araci.Services;
using Araci.ViewModels;
using System.Windows;

namespace Araci.Applications.Commands
{
    public class SelectCommand : ICommandHandler
    {
        public void Execute(ElementoViewModel vm, Vector? delta = null)
        {
            SelectionService.Selecionar(vm);
        }
    }
}