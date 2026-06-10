using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverRetanguloDoTipoPranchaUseCase
    {
        private const double MinDeltaSquared = 0.0001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public MoverRetanguloDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Mover(Guid tipoId, Guid retanguloId, double deltaX, double deltaY)
        {
            if (!TemDeltaValido(deltaX, deltaY))
                return false;

            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            return AlterarPosicao(tipo, retangulo, retangulo.X + deltaX, retangulo.Y + deltaY);
        }

        public bool AlterarPosicao(Guid tipoId, Guid retanguloId, double x, double y)
        {
            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            return AlterarPosicao(tipo, retangulo, x, y);
        }

        private bool AlterarPosicao(ProjectSheetType tipo, ProjectSheetTemplateRectangle retangulo, double x, double y)
        {
            if (!ValorFinito(x) || !ValorFinito(y))
                return false;

            double deltaSquared = DistanciaQuadrada(retangulo.X, retangulo.Y, x, y);

            if (deltaSquared < MinDeltaSquared)
                return false;

            var estadoAnterior = ProjectSheetTemplateRectanglePositionState.FromRectangle(retangulo);
            var estadoNovo = new ProjectSheetTemplateRectanglePositionState(x, y);

            _commands.Execute(new UpdateProjectSheetTypeRectanglePropertyCommand<ProjectSheetTemplateRectanglePositionState>(
                _document,
                tipo,
                retangulo.Id,
                (r, value) => value.Aplicar(r),
                estadoAnterior,
                estadoNovo));

            return true;
        }

        private bool TryGetRetangulo(Guid tipoId, Guid retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo)
        {
            tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId)!;
            retangulo = tipo?.Retangulos.FirstOrDefault(r => r.Id == retanguloId)!;

            return tipo != null && retangulo != null;
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