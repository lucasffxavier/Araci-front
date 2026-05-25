using System;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class MoveElementoCommand : IUndoableCommand
    {
        private readonly Elemento _elemento;
        private readonly ElementoEstado _antes;
        private readonly ElementoEstado _depois;
        private readonly Action<Elemento>? _onStateApplied;

        public MoveElementoCommand(
            Elemento elemento,
            ElementoEstado antes,
            ElementoEstado depois,
            Action<Elemento>? onStateApplied = null)
        {
            _elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
            _antes = antes;
            _depois = depois;
            _onStateApplied = onStateApplied;
        }

        public void Execute()
        {
            Aplicar(_depois);
        }

        public void Undo()
        {
            Aplicar(_antes);
        }

        public void Redo()
        {
            Aplicar(_depois);
        }

        private void Aplicar(ElementoEstado estado)
        {
            estado.AplicarEm(_elemento);
            _onStateApplied?.Invoke(_elemento);
        }
    }
}
