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

        private readonly EditorContext
            _context;

        // =========================
        // CONSTRUTOR
        // =========================

        public DeleteElementCommand(
            ElementoViewModel vm,
            EditorContext context)
        {
            _vm = vm;

            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            _context.Document
                .RemoverElemento(_vm.Modelo);

            _context.Selection
                .Deselecionar(_vm);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            _context.Viewport
                ?.RegistrarViewModel(_vm);

            _context.Document
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
