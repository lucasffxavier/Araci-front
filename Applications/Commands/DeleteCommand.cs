using Araci.Services;
using Araci.ViewModels;
using System.Windows;

namespace Araci.Applications.Commands
{
    public class DeleteCommand : ICommandHandler
    {
        public void Execute(ElementoViewModel vm, Vector? delta = null)
        {
            AppServices.Viewport?
                .RemoverElemento(vm);

            SelectionService.Limpar();
        }
    }
}
