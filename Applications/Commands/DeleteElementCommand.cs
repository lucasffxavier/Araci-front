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
            AppServices.Document
                .RemoverElemento(_vm);

            // =========================
            // REMOVE APENAS O ITEM
            // =========================

            SelectionService
                .Deselecionar(_vm);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            AppServices.Document
                .AdicionarElemento(_vm);
        }
    }
}