using System;
using Araci.Models.Tipos;

namespace Araci.Core.Commands
{
    public sealed class UpdateLineAnnotationTypePropertyCommand<T> : IUndoableCommand
    {
        private readonly TipoLinhaAnotativa _tipo;
        private readonly Action<TipoLinhaAnotativa, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateLineAnnotationTypePropertyCommand(
            TipoLinhaAnotativa tipo,
            Action<TipoLinhaAnotativa, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _aplicar = aplicar ?? throw new ArgumentNullException(nameof(aplicar));
            _valorAnterior = valorAnterior;
            _valorNovo = valorNovo;
        }

        public void Execute()
        {
            _aplicar(_tipo, _valorNovo);
        }

        public void Undo()
        {
            _aplicar(_tipo, _valorAnterior);
        }

        public void Redo()
        {
            Execute();
        }
    }
}