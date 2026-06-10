using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class RemoveProjectSheetTypeRectangleCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateRectangle _retangulo;
        private int _index;

        public RemoveProjectSheetTypeRectangleCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateRectangle retangulo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _retangulo = retangulo ?? throw new ArgumentNullException(nameof(retangulo));
            _index = Math.Max(0, _tipo.Retangulos.FindIndex(r => r.Id == _retangulo.Id));
        }

        public void Execute()
        {
            int index = _tipo.Retangulos.FindIndex(r => r.Id == _retangulo.Id);

            if (index >= 0)
            {
                _index = index;
                _tipo.Retangulos.RemoveAt(index);
            }

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            if (_tipo.Retangulos.Any(r => r.Id == _retangulo.Id))
            {
                _document.AtualizarPropriedadesTipoPrancha(_tipo);
                return;
            }

            int index = Math.Max(0, Math.Min(_index, _tipo.Retangulos.Count));
            _tipo.Retangulos.Insert(index, _retangulo);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}