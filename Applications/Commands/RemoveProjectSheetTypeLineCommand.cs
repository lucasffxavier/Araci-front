using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class RemoveProjectSheetTypeLineCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateLine _linha;
        private int _index;

        public RemoveProjectSheetTypeLineCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateLine linha)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
            _index = Math.Max(0, _tipo.Linhas.FindIndex(l => l.Id == _linha.Id));
        }

        public void Execute()
        {
            int index = _tipo.Linhas.FindIndex(l => l.Id == _linha.Id);

            if (index >= 0)
            {
                _index = index;
                _tipo.Linhas.RemoveAt(index);
            }

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            if (_tipo.Linhas.Any(l => l.Id == _linha.Id))
            {
                _document.AtualizarPropriedadesTipoPrancha(_tipo);
                return;
            }

            int index = Math.Max(0, Math.Min(_index, _tipo.Linhas.Count));
            _tipo.Linhas.Insert(index, _linha);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}