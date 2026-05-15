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
            if (AppServices.Viewport != null)
            {
                AppServices.Viewport
                    .AdicionarElemento(_elemento);

                return;
            }

            AppServices.Document
                .AdicionarElemento(_elemento.Modelo);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            if (AppServices.Viewport != null)
            {
                AppServices.Viewport
                    .RemoverElemento(_elemento);

                return;
            }

            AppServices.Document
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
