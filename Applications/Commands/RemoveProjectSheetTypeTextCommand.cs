using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class RemoveProjectSheetTypeTextCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateText _texto;
        private int _index;

        public RemoveProjectSheetTypeTextCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateText texto)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _texto = texto ?? throw new ArgumentNullException(nameof(texto));
            _index = Math.Max(0, _tipo.Textos.FindIndex(t => t.Id == _texto.Id));
        }

        public void Execute()
        {
            int index = _tipo.Textos.FindIndex(t => t.Id == _texto.Id);

            if (index >= 0)
            {
                _index = index;
                _tipo.Textos.RemoveAt(index);
            }

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            if (_tipo.Textos.Any(t => t.Id == _texto.Id))
            {
                _document.AtualizarPropriedadesTipoPrancha(_tipo);
                return;
            }

            int index = Math.Max(0, Math.Min(_index, _tipo.Textos.Count));
            _tipo.Textos.Insert(index, _texto);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}