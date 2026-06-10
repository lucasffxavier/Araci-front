using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class AddProjectSheetTypeLineCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateLine _linha;

        public AddProjectSheetTypeLineCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateLine linha)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
        }

        public void Execute()
        {
            if (!_tipo.Linhas.Any(l => l.Id == _linha.Id))
                _tipo.Linhas.Add(_linha);

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            _tipo.Linhas.RemoveAll(l => l.Id == _linha.Id);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}
