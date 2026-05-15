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

        // =========================
        // CONSTRUTOR
        // =========================

        public AddElementoCommand(
            ElementoViewModel elemento)
        {
            _elemento = elemento;
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            AppServices.Document
                .AdicionarElemento(_elemento);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            AppServices.Document
                .RemoverElemento(_elemento);
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