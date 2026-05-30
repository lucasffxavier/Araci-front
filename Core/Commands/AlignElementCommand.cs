using System;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class AlignElementCommand : IUndoableCommand
    {
        private readonly ElementoViewModel _elemento;
        private readonly ElementoEstado _antes;
        private readonly ElementoEstado _depois;

        public AlignElementCommand(ElementoViewModel elemento, ElementoEstado antes, ElementoEstado depois)
        {
            _elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
            _antes = antes;
            _depois = depois;
        }

        public void Execute()
        {
            _elemento.AplicarEstado(_depois);
        }

        public void Undo()
        {
            _elemento.AplicarEstado(_antes);
        }

        public void Redo()
        {
            Execute();
        }
    }
}