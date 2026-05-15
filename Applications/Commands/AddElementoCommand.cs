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
            EditorContext context)
        {
            _elemento = elemento;

            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            _context.Viewport
                ?.RegistrarViewModel(_elemento);

            _context.Document
                .AdicionarElemento(_elemento.Modelo);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
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
