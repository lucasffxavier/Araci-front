using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetTemplateRectangle
    {
        public const double DefaultWidth = 100.0;
        public const double DefaultHeight = 50.0;
        public const double MinDimension = 0.0001;

        private double _largura = DefaultWidth;
        private double _altura = DefaultHeight;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public string TipoLinhaNome { get; set; } = "Linha contínua";
        public string TipoLinhaFamilia { get; set; } = "Anotações";
        public string TipoLinhaCategoria { get; set; } = "Linhas";
        public string Stroke { get; set; } = "#FF000000";
        public double StrokeThickness { get; set; } = 1.0;
        public bool Visible { get; set; } = true;

        public double Largura
        {
            get => _largura;
            set => _largura = NormalizarDimensao(value, DefaultWidth);
        }

        public double Altura
        {
            get => _altura;
            set => _altura = NormalizarDimensao(value, DefaultHeight);
        }

        public bool PossuiTipoLinha =>
            !string.IsNullOrWhiteSpace(TipoLinhaNome) &&
            !string.IsNullOrWhiteSpace(TipoLinhaFamilia) &&
            !string.IsNullOrWhiteSpace(TipoLinhaCategoria);

        public void DefinirTipoLinha(string? nomeTipo, string? familia, string? categoria)
        {
            TipoLinhaNome = NormalizarTexto(nomeTipo);
            TipoLinhaFamilia = NormalizarTexto(familia);
            TipoLinhaCategoria = NormalizarTexto(categoria);
        }

        public bool TipoLinhaIgual(string? nomeTipo, string? familia, string? categoria)
        {
            return string.Equals(TipoLinhaNome, NormalizarTexto(nomeTipo), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TipoLinhaFamilia, NormalizarTexto(familia), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TipoLinhaCategoria, NormalizarTexto(categoria), StringComparison.OrdinalIgnoreCase);
        }

        public ProjectSheetTemplateRectangle CriarCopia(bool gerarNovoId)
        {
            return new ProjectSheetTemplateRectangle
            {
                Id = gerarNovoId ? Guid.NewGuid() : Id,
                Nome = Nome,
                X = X,
                Y = Y,
                Largura = Largura,
                Altura = Altura,
                TipoLinhaNome = TipoLinhaNome,
                TipoLinhaFamilia = TipoLinhaFamilia,
                TipoLinhaCategoria = TipoLinhaCategoria,
                Stroke = Stroke,
                StrokeThickness = StrokeThickness,
                Visible = Visible
            };
        }

        private static string NormalizarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        }

        private static double NormalizarDimensao(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < MinDimension
                ? fallback
                : valor;
        }
    }
}