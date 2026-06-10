using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Models.Tipos;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverRetanguloDoTipoPranchaUseCase
    {
        private const double MinDeltaSquared = 0.0001;
        private const double MinStrokeThickness = 0.1;

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

        public bool AlterarNome(Guid tipoId, Guid retanguloId, string nome)
        {
            string nomeNormalizado = NormalizarNome(nome);

            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            if (string.Equals(retangulo.Nome, nomeNormalizado, StringComparison.Ordinal))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeRectanglePropertyCommand<string>(
                _document,
                tipo,
                retangulo.Id,
                (r, value) => r.Nome = value,
                retangulo.Nome,
                nomeNormalizado));

            return true;
        }

        public bool AlterarTipoGrafico(Guid tipoId, Guid retanguloId, TipoLinhaAnotativa tipoLinha)
        {
            if (tipoLinha == null)
                return false;

            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            if (retangulo.TipoLinhaIgual(tipoLinha.NomeTipo, tipoLinha.Familia, tipoLinha.Categoria))
                return false;

            var estadoAnterior = ProjectSheetTemplateRectangleGraphicTypeState.FromRectangle(retangulo);
            var estadoNovo = new ProjectSheetTemplateRectangleGraphicTypeState(
                tipoLinha.NomeTipo,
                tipoLinha.Familia,
                tipoLinha.Categoria);

            _commands.Execute(new UpdateProjectSheetTypeRectanglePropertyCommand<ProjectSheetTemplateRectangleGraphicTypeState>(
                _document,
                tipo,
                retangulo.Id,
                (r, value) => value.Aplicar(r),
                estadoAnterior,
                estadoNovo));

            return true;
        }

        public bool AlterarLargura(Guid tipoId, Guid retanguloId, double largura)
        {
            if (!DimensaoValida(largura))
                return false;

            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            if (Math.Abs(retangulo.Largura - largura) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeRectanglePropertyCommand<double>(
                _document,
                tipo,
                retangulo.Id,
                (r, value) => r.Largura = value,
                retangulo.Largura,
                largura));

            return true;
        }

        public bool AlterarAltura(Guid tipoId, Guid retanguloId, double altura)
        {
            if (!DimensaoValida(altura))
                return false;

            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            if (Math.Abs(retangulo.Altura - altura) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeRectanglePropertyCommand<double>(
                _document,
                tipo,
                retangulo.Id,
                (r, value) => r.Altura = value,
                retangulo.Altura,
                altura));

            return true;
        }

        public bool AlterarStroke(Guid tipoId, Guid retanguloId, string stroke)
        {
            if (!TryNormalizeStroke(stroke, out string normalizedStroke))
                return false;

            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            if (string.Equals(retangulo.Stroke, normalizedStroke, StringComparison.OrdinalIgnoreCase))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeRectanglePropertyCommand<string>(
                _document,
                tipo,
                retangulo.Id,
                (r, value) => r.Stroke = value,
                retangulo.Stroke,
                normalizedStroke));

            return true;
        }

        public bool AlterarEspessura(Guid tipoId, Guid retanguloId, double strokeThickness)
        {
            if (!StrokeThicknessValida(strokeThickness))
                return false;

            if (!TryGetRetangulo(tipoId, retanguloId, out ProjectSheetType tipo, out ProjectSheetTemplateRectangle retangulo))
                return false;

            if (Math.Abs(retangulo.StrokeThickness - strokeThickness) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeRectanglePropertyCommand<double>(
                _document,
                tipo,
                retangulo.Id,
                (r, value) => r.StrokeThickness = value,
                retangulo.StrokeThickness,
                strokeThickness));

            return true;
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

        private static bool DimensaoValida(double value)
        {
            return ValorFinito(value) && value >= ProjectSheetTemplateRectangle.MinDimension;
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