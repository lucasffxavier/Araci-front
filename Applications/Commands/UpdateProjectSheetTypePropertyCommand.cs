using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectSheetTypePropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly Action<ProjectSheetType, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectSheetTypePropertyCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            Action<ProjectSheetType, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _aplicar = aplicar ?? throw new ArgumentNullException(nameof(aplicar));
            _valorAnterior = valorAnterior;
            _valorNovo = valorNovo;
        }

        public void Execute()
        {
            Aplicar(_valorNovo);
        }

        public void Undo()
        {
            Aplicar(_valorAnterior);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(T valor)
        {
            _aplicar(_tipo, valor);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }
    }
}
