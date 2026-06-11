using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class AddProjectSheetTypeTextCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateText _texto;

        public AddProjectSheetTypeTextCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateText texto)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _texto = texto ?? throw new ArgumentNullException(nameof(texto));
        }

        public void Execute()
        {
            if (!_tipo.Textos.Any(t => t.Id == _texto.Id))
                _tipo.Textos.Add(_texto);

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            _tipo.Textos.RemoveAll(t => t.Id == _texto.Id);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}