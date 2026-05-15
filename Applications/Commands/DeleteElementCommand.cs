using Araci.Services;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class DeleteElementCommand
        : IUndoableCommand
    {
        // =========================
        // ELEMENTO
        // =========================

        private readonly ElementoViewModel
            _vm;

        // =========================
        // CONSTRUTOR
        // =========================

        public DeleteElementCommand(
            ElementoViewModel vm)
        {
            _vm = vm;
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            if (AppServices.Viewport != null)
            {
                AppServices.Viewport
                    .RemoverElemento(_vm);
            }
            else
            {
                AppServices.Document
                    .RemoverElemento(_vm.Modelo);
            }

            SelectionService
                .Deselecionar(_vm);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            if (AppServices.Viewport != null)
            {
                AppServices.Viewport
                    .AdicionarElemento(_vm);

                return;
            }

            AppServices.Document
                .AdicionarElemento(_vm.Modelo);
        }

        // =========================
        // REDO
        // =========================

        public void Redo()
        {
            Execute();
        }
    }
}
