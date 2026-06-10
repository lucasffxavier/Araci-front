using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverLinhaDoTipoPranchaUseCase
    {
        private const double MinDeltaSquared = 0.0001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public MoverLinhaDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Mover(Guid tipoId, Guid linhaId, double deltaX, double deltaY)
        {
            if (!TemDeltaValido(deltaX, deltaY))
                return false;

            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return false;

            ProjectSheetTemplateLine? linha = tipo.Linhas.FirstOrDefault(l => l.Id == linhaId);

            if (linha == null)
                return false;

            double newX1 = linha.X1 + deltaX;
            double newY1 = linha.Y1 + deltaY;
            double newX2 = linha.X2 + deltaX;
            double newY2 = linha.Y2 + deltaY;

            _commands.Execute(new MoveProjectSheetTypeLineCommand(
                _document,
                tipo,
                linha.Id,
                linha.X1,
                linha.Y1,
                linha.X2,
                linha.Y2,
                newX1,
                newY1,
                newX2,
                newY2));

            return true;
        }

        private static bool TemDeltaValido(double deltaX, double deltaY)
        {
            if (double.IsNaN(deltaX) || double.IsNaN(deltaY) || double.IsInfinity(deltaX) || double.IsInfinity(deltaY))
                return false;

            return deltaX * deltaX + deltaY * deltaY >= MinDeltaSquared;
        }
    }
}