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
        private const double MinStrokeThickness = 0.1;

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

            if (!TryGetLinha(tipoId, linhaId, out ProjectSheetType tipo, out ProjectSheetTemplateLine linha))
                return false;

            return AlterarCoordenadas(
                tipo,
                linha,
                linha.X1 + deltaX,
                linha.Y1 + deltaY,
                linha.X2 + deltaX,
                linha.Y2 + deltaY);
        }

        public bool AlterarCoordenadas(Guid tipoId, Guid linhaId, double x1, double y1, double x2, double y2)
        {
            if (!TryGetLinha(tipoId, linhaId, out ProjectSheetType tipo, out ProjectSheetTemplateLine linha))
                return false;

            return AlterarCoordenadas(tipo, linha, x1, y1, x2, y2);
        }

        public bool AlterarStroke(Guid tipoId, Guid linhaId, string stroke)
        {
            if (!TryNormalizeStroke(stroke, out string normalizedStroke))
                return false;

            if (!TryGetLinha(tipoId, linhaId, out ProjectSheetType tipo, out ProjectSheetTemplateLine linha))
                return false;

            if (string.Equals(linha.Stroke, normalizedStroke, StringComparison.OrdinalIgnoreCase))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeLinePropertyCommand<string>(
                _document,
                tipo,
                linha.Id,
                (l, value) => l.Stroke = value,
                linha.Stroke,
                normalizedStroke));

            return true;
        }

        public bool AlterarEspessura(Guid tipoId, Guid linhaId, double strokeThickness)
        {
            if (!StrokeThicknessValida(strokeThickness))
                return false;

            if (!TryGetLinha(tipoId, linhaId, out ProjectSheetType tipo, out ProjectSheetTemplateLine linha))
                return false;

            if (Math.Abs(linha.StrokeThickness - strokeThickness) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeLinePropertyCommand<double>(
                _document,
                tipo,
                linha.Id,
                (l, value) => l.StrokeThickness = value,
                linha.StrokeThickness,
                strokeThickness));

            return true;
        }

        private bool AlterarCoordenadas(ProjectSheetType tipo, ProjectSheetTemplateLine linha, double x1, double y1, double x2, double y2)
        {
            if (!CoordenadasValidas(x1, y1, x2, y2))
                return false;

            double deltaSquared =
                DistanciaQuadrada(linha.X1, linha.Y1, x1, y1) +
                DistanciaQuadrada(linha.X2, linha.Y2, x2, y2);

            if (deltaSquared < MinDeltaSquared)
                return false;

            _commands.Execute(new MoveProjectSheetTypeLineCommand(
                _document,
                tipo,
                linha.Id,
                linha.X1,
                linha.Y1,
                linha.X2,
                linha.Y2,
                x1,
                y1,
                x2,
                y2));

            return true;
        }

        private bool TryGetLinha(Guid tipoId, Guid linhaId, out ProjectSheetType tipo, out ProjectSheetTemplateLine linha)
        {
            tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId)!;
            linha = tipo?.Linhas.FirstOrDefault(l => l.Id == linhaId)!;

            return tipo != null && linha != null;
        }

        private static bool TemDeltaValido(double deltaX, double deltaY)
        {
            if (!ValorFinito(deltaX) || !ValorFinito(deltaY))
                return false;

            return deltaX * deltaX + deltaY * deltaY >= MinDeltaSquared;
        }

        private static bool CoordenadasValidas(double x1, double y1, double x2, double y2)
        {
            if (!ValorFinito(x1) || !ValorFinito(y1) || !ValorFinito(x2) || !ValorFinito(y2))
                return false;

            return DistanciaQuadrada(x1, y1, x2, y2) >= MinDeltaSquared;
        }

        private static bool StrokeThicknessValida(double strokeThickness)
        {
            return ValorFinito(strokeThickness) && strokeThickness >= MinStrokeThickness;
        }

        private static bool TryNormalizeStroke(string stroke, out string normalizedStroke)
        {
            normalizedStroke = (stroke ?? string.Empty).Trim();
            return !string.IsNullOrWhiteSpace(normalizedStroke);
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