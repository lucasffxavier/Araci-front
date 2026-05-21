using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class MoveElementoCommand : IUndoableCommand
    {
        private readonly ElementoViewModel _vm;
        private readonly ElementoEstado _antes;
        private readonly ElementoEstado _depois;

        public MoveElementoCommand(
            ElementoViewModel vm,
            ElementoEstado antes,
            ElementoEstado depois)
        {
            _vm = vm;
            _antes = antes;
            _depois = depois;
        }

        public void Execute()
        {
            _vm.AplicarEstado(_depois);
        }

        public void Undo()
        {
            _vm.AplicarEstado(_antes);
        }

        public void Redo()
        {
            _vm.AplicarEstado(_depois);
        }
    }
}