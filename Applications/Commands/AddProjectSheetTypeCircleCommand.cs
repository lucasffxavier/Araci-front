using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class AddProjectSheetTypeCircleCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateCircle _circulo;

        public AddProjectSheetTypeCircleCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            ProjectSheetTemplateCircle circulo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _circulo = circulo ?? throw new ArgumentNullException(nameof(circulo));
        }

        public void Execute()
        {
            if (!_tipo.Circulos.Any(c => c.Id == _circulo.Id))
                _tipo.Circulos.Add(_circulo);

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Undo()
        {
            _tipo.Circulos.RemoveAll(c => c.Id == _circulo.Id);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }

        public void Redo()
        {
            Execute();
        }
    }
}