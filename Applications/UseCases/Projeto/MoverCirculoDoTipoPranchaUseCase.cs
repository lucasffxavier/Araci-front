using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverCirculoDoTipoPranchaUseCase
    {
        private const double MinDeltaSquared = 0.0001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public MoverCirculoDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Mover(Guid tipoId, Guid circuloId, double deltaX, double deltaY)
        {
            if (!TemDeltaValido(deltaX, deltaY))
                return false;

            if (!TryGetCirculo(tipoId, circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo))
                return false;

            return AlterarPosicao(tipo, circulo, circulo.X + deltaX, circulo.Y + deltaY);
        }

        public bool AlterarPosicao(Guid tipoId, Guid circuloId, double x, double y)
        {
            if (!TryGetCirculo(tipoId, circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo))
                return false;

            return AlterarPosicao(tipo, circulo, x, y);
        }

        private bool AlterarPosicao(ProjectSheetType tipo, ProjectSheetTemplateCircle circulo, double x, double y)
        {
            if (!ValorFinito(x) || !ValorFinito(y))
                return false;

            double deltaSquared = DistanciaQuadrada(circulo.X, circulo.Y, x, y);

            if (deltaSquared < MinDeltaSquared)
                return false;

            var estadoAnterior = ProjectSheetTemplateCirclePositionState.FromCircle(circulo);
            var estadoNovo = new ProjectSheetTemplateCirclePositionState(x, y);

            _commands.Execute(new UpdateProjectSheetTypeCirclePropertyCommand<ProjectSheetTemplateCirclePositionState>(
                _document,
                tipo,
                circulo.Id,
                (c, value) => value.Aplicar(c),
                estadoAnterior,
                estadoNovo));

            return true;
        }

        private bool TryGetCirculo(Guid tipoId, Guid circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo)
        {
            tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId)!;
            circulo = tipo?.Circulos.FirstOrDefault(c => c.Id == circuloId)!;

            return tipo != null && circulo != null;
        }

        private static bool TemDeltaValido(double deltaX, double deltaY)
        {
            if (!ValorFinito(deltaX) || !ValorFinito(deltaY))
                return false;

            return deltaX * deltaX + deltaY * deltaY >= MinDeltaSquared;
        }

        private static bool ValorFinito(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static double DistanciaQuadrada(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return dx * dx + dy * dy;
        }
    }
}