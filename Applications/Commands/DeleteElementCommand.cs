using System;
using Araci.Core.Documents;
using Araci.Models;

namespace Araci.Core.Commands
{
    public class DeleteElementCommand : IUndoableCommand
    {
        private readonly Elemento _elemento;
        private readonly AraciDocument _document;

        public DeleteElementCommand(
            Elemento elemento,
            AraciDocument document)
        {
            _elemento = elemento
                ?? throw new ArgumentNullException(nameof(elemento));

            _document = document
                ?? throw new ArgumentNullException(nameof(document));
        }

        public void Execute()
        {
            _document.RemoverElemento(_elemento);
        }

        public void Undo()
        {
            _document.AdicionarElementoPreservandoVista(_elemento);
        }

        public void Redo()
        {
            Execute();
        }
    }
}
