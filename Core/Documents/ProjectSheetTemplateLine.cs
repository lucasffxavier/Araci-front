using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetTemplateLine
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public string TipoLinhaNome { get; set; } = "Linha contínua";
        public string TipoLinhaFamilia { get; set; } = "Anotações";
        public string TipoLinhaCategoria { get; set; } = "Linhas";
        public string Stroke { get; set; } = "#FF000000";
        public double StrokeThickness { get; set; } = 1.0;
        public bool Visible { get; set; } = true;

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

        public ProjectSheetTemplateLine CriarCopia(bool gerarNovoId)
        {
            return new ProjectSheetTemplateLine
            {
                Id = gerarNovoId ? Guid.NewGuid() : Id,
                Nome = Nome,
                X1 = X1,
                Y1 = Y1,
                X2 = X2,
                Y2 = Y2,
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
    }
}