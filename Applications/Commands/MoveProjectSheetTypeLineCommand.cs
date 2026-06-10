using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class MoveProjectSheetTypeLineCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly Guid _linhaId;
        private readonly double _oldX1;
        private readonly double _oldY1;
        private readonly double _oldX2;
        private readonly double _oldY2;
        private readonly double _newX1;
        private readonly double _newY1;
        private readonly double _newX2;
        private readonly double _newY2;

        public MoveProjectSheetTypeLineCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            Guid linhaId,
            double oldX1,
            double oldY1,
            double oldX2,
            double oldY2,
            double newX1,
            double newY1,
            double newX2,
            double newY2)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _linhaId = linhaId;
            _oldX1 = oldX1;
            _oldY1 = oldY1;
            _oldX2 = oldX2;
            _oldY2 = oldY2;
            _newX1 = newX1;
            _newY1 = newY1;
            _newX2 = newX2;
            _newY2 = newY2;
        }

        public void Execute()
        {
            Aplicar(_newX1, _newY1, _newX2, _newY2);
        }

        public void Undo()
        {
            Aplicar(_oldX1, _oldY1, _oldX2, _oldY2);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(double x1, double y1, double x2, double y2)
        {
            ProjectSheetTemplateLine? linha = _tipo.Linhas.FirstOrDefault(l => l.Id == _linhaId);

            if (linha == null)
                return;

            linha.X1 = x1;
            linha.Y1 = y1;
            linha.X2 = x2;
            linha.Y2 = y2;

            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }
    }
}