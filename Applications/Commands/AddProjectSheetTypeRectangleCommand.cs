using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class AddProjectSheetTypeRectangleCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateRectangle _retangulo;

        public AddProjectSheetTypeRectangleCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateRectangle retangulo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _retangulo = retangulo ?? throw new ArgumentNullException(nameof(retangulo));
        }

        public void Execute()
        {
            if (!_tipo.Retangulos.Any(r => r.Id == _retangulo.Id))
                _tipo.Retangulos.Add(_retangulo);

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            _tipo.Retangulos.RemoveAll(r => r.Id == _retangulo.Id);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}