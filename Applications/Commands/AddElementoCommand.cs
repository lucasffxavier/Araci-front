using Araci.Services;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class AddElementoCommand
        : IUndoableCommand
    {
        // =========================
        // ELEMENTO
        // =========================

        private readonly ElementoViewModel
            _elemento;

        private readonly EditorContext
            _context;

        // =========================
        // CONSTRUTOR
        // =========================

        public AddElementoCommand(
            ElementoViewModel elemento,
            EditorContext? context = null)
        {
            _elemento = elemento;

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
                    .AdicionarElemento(_elemento);

                return;
            }

            _context.Document
                .AdicionarElemento(_elemento.Modelo);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            if (_context.Viewport != null)
            {
                _context.Viewport
                    .RemoverElemento(_elemento);

                return;
            }

            _context.Document
                .RemoverElemento(_elemento.Modelo);
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
