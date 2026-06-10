using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetTemplateCircle
    {
        public const double DefaultRadius = 50.0;
        public const double MinRadius = 0.0001;

        private double _raio = DefaultRadius;

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

        public double Raio
        {
            get => _raio;
            set => _raio = NormalizarRaio(value, DefaultRadius);
        }

        public double Diametro => Raio * 2.0;

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

        public ProjectSheetTemplateCircle CriarCopia(bool gerarNovoId)
        {
            return new ProjectSheetTemplateCircle
            {
                Id = gerarNovoId ? Guid.NewGuid() : Id,
                Nome = Nome,
                X = X,
                Y = Y,
                Raio = Raio,
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

        private static double NormalizarRaio(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < MinRadius
                ? fallback
                : valor;
        }
    }
}