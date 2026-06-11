using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Models.Tipos;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverCirculoDoTipoPranchaUseCase
    {
        private const double MinDeltaSquared = 0.0001;
        private const double MinStrokeThickness = 0.1;

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

        public bool AlterarNome(Guid tipoId, Guid circuloId, string nome)
        {
            string nomeNormalizado = NormalizarNome(nome);

            if (!TryGetCirculo(tipoId, circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo))
                return false;

            if (string.Equals(circulo.Nome, nomeNormalizado, StringComparison.Ordinal))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeCirclePropertyCommand<string>(
                _document,
                tipo,
                circulo.Id,
                (c, value) => c.Nome = value,
                circulo.Nome,
                nomeNormalizado));

            return true;
        }

        public bool AlterarTipoGrafico(Guid tipoId, Guid circuloId, TipoLinhaAnotativa tipoLinha)
        {
            if (tipoLinha == null)
                return false;

            if (!TryGetCirculo(tipoId, circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo))
                return false;

            if (circulo.TipoLinhaIgual(tipoLinha.NomeTipo, tipoLinha.Familia, tipoLinha.Categoria))
                return false;

            var estadoAnterior = ProjectSheetTemplateCircleGraphicTypeState.FromCircle(circulo);
            var estadoNovo = new ProjectSheetTemplateCircleGraphicTypeState(
                tipoLinha.NomeTipo,
                tipoLinha.Familia,
                tipoLinha.Categoria);

            _commands.Execute(new UpdateProjectSheetTypeCirclePropertyCommand<ProjectSheetTemplateCircleGraphicTypeState>(
                _document,
                tipo,
                circulo.Id,
                (c, value) => value.Aplicar(c),
                estadoAnterior,
                estadoNovo));

            return true;
        }

        public bool AlterarRaio(Guid tipoId, Guid circuloId, double raio)
        {
            if (!RaioValido(raio))
                return false;

            if (!TryGetCirculo(tipoId, circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo))
                return false;

            if (Math.Abs(circulo.Raio - raio) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeCirclePropertyCommand<double>(
                _document,
                tipo,
                circulo.Id,
                (c, value) => c.Raio = value,
                circulo.Raio,
                raio));

            return true;
        }

        public bool AlterarStroke(Guid tipoId, Guid circuloId, string stroke)
        {
            if (!TryNormalizeStroke(stroke, out string normalizedStroke))
                return false;

            if (!TryGetCirculo(tipoId, circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo))
                return false;

            if (string.Equals(circulo.Stroke, normalizedStroke, StringComparison.OrdinalIgnoreCase))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeCirclePropertyCommand<string>(
                _document,
                tipo,
                circulo.Id,
                (c, value) => c.Stroke = value,
                circulo.Stroke,
                normalizedStroke));

            return true;
        }

        public bool AlterarEspessura(Guid tipoId, Guid circuloId, double strokeThickness)
        {
            if (!StrokeThicknessValida(strokeThickness))
                return false;

            if (!TryGetCirculo(tipoId, circuloId, out ProjectSheetType tipo, out ProjectSheetTemplateCircle circulo))
                return false;

            if (Math.Abs(circulo.StrokeThickness - strokeThickness) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeCirclePropertyCommand<double>(
                _document,
                tipo,
                circulo.Id,
                (c, value) => c.StrokeThickness = value,
                circulo.StrokeThickness,
                strokeThickness));

            return true;
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

        private static bool RaioValido(double value)
        {
            return ValorFinito(value) && value >= ProjectSheetTemplateCircle.MinRadius;
        }

        private static bool StrokeThicknessValida(double strokeThickness)
        {
            return ValorFinito(strokeThickness) && strokeThickness >= MinStrokeThickness;
        }

        private static bool TryNormalizeStroke(string stroke, out string normalizedStroke)
        {
            normalizedStroke = TipoLinhaAnotativa.NormalizarCor(stroke);
            return !string.IsNullOrWhiteSpace(normalizedStroke);
        }

        private static string NormalizarNome(string nome)
        {
            return string.IsNullOrWhiteSpace(nome) ? string.Empty : nome.Trim();
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