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
            EditorContext? context = null)
        {
            _vm = vm;

            _context = context
                ?? AppServices.Current;
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            if (_context.Viewport != null)
            {
                _context.Viewport
                    .RemoverElemento(_vm);
            }
            else
            {
                _context.Document
                    .RemoverElemento(_vm.Modelo);
            }

            _context.Selection
                .Deselecionar(_vm);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            if (_context.Viewport != null)
            {
                _context.Viewport
                    .AdicionarElemento(_vm);

                return;
            }

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
