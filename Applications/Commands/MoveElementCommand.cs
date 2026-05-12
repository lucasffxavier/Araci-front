using System.Windows;

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
            Vector delta)
        {
            _vm = vm;

            _estadoInicial =
                vm.CapturarEstado();

            vm.Mover(delta);

            _estadoFinal =
                vm.CapturarEstado();

            vm.AplicarEstado(
                _estadoInicial);
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
    }
}