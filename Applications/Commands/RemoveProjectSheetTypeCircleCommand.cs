using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class RemoveProjectSheetTypeCircleCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateCircle _circulo;
        private int _index;

        public RemoveProjectSheetTypeCircleCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateCircle circulo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _circulo = circulo ?? throw new ArgumentNullException(nameof(circulo));
            _index = Math.Max(0, _tipo.Circulos.FindIndex(c => c.Id == _circulo.Id));
        }

        public void Execute()
        {
            int index = _tipo.Circulos.FindIndex(c => c.Id == _circulo.Id);

            if (index >= 0)
            {
                _index = index;
                _tipo.Circulos.RemoveAt(index);
            }

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            if (_tipo.Circulos.Any(c => c.Id == _circulo.Id))
            {
                _document.AtualizarPropriedadesTipoPrancha(_tipo);
                return;
            }

            int index = Math.Max(0, Math.Min(_index, _tipo.Circulos.Count));
            _tipo.Circulos.Insert(index, _circulo);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}