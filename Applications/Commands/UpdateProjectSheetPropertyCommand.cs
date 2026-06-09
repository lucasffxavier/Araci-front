using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectSheetPropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheet _prancha;
        private readonly Action<ProjectSheet, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectSheetPropertyCommand(
            AraciDocument document,
            ProjectSheet prancha,
            Action<ProjectSheet, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _prancha = prancha ?? throw new ArgumentNullException(nameof(prancha));
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
            _aplicar(_prancha, valor);
            _document.AtualizarPropriedadesPrancha(_prancha);
        }
    }
}
