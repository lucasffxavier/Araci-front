using Araci.Services;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class DeleteElementCommand
        : IUndoableCommand
    {
        private readonly ElementoViewModel
            _vm;

        public DeleteElementCommand(
            ElementoViewModel vm)
        {
            _vm = vm;
        }

        public void Execute()
        {
            AppServices.Document
                .RemoverElemento(_vm);

            SelectionService.Limpar();
        }

        public void Undo()
        {
            AppServices.Document
                .AdicionarElemento(_vm);
        }
    }
}