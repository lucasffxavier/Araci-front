using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class MoveElementCommand
        : IUndoableCommand
    {
        // =========================
        // ELEMENTO
        // =========================

        private readonly ElementoViewModel
            _vm;

        // =========================
        // ESTADOS
        // =========================

        private readonly ElementoEstado
            _estadoInicial;

        private readonly ElementoEstado
            _estadoFinal;

        // =========================
        // CONSTRUTOR
        // =========================

        public MoveElementCommand(
            ElementoViewModel vm,
            ElementoEstado estadoInicial,
            ElementoEstado estadoFinal)
        {
            _vm = vm;

            _estadoInicial =
                estadoInicial;

            _estadoFinal =
                estadoFinal;
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            _vm.AplicarEstado(
                _estadoFinal);
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            _vm.AplicarEstado(
                _estadoInicial);
        }

        // =========================
        // REDO
        // =========================

        public void Redo()
        {
            _vm.AplicarEstado(
                _estadoFinal);
        }
    }
}