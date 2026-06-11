using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetTemplateText
    {
        public const string DefaultText = "Texto";
        public const double DefaultBoxWidth = 200.0;
        public const double MinBoxWidth = 20.0;
        public const string DefaultTextTypeName = "Texto padrão";
        public const string DefaultTextTypeFamily = "Anotações";
        public const string DefaultTextTypeCategory = "Textos";
        public const string DefaultTextColor = "#FF000000";
        public const string DefaultFont = "Arial";
        public const double DefaultTextHeight = 14.0;
        public const double MinTextHeight = 0.0001;
        public const string DefaultHorizontalAlignment = "Esquerda";

        private string _texto = DefaultText;
        private double _larguraCaixa = DefaultBoxWidth;
        private double _alturaTexto = DefaultTextHeight;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public string TipoTextoNome { get; set; } = DefaultTextTypeName;
        public string TipoTextoFamilia { get; set; } = DefaultTextTypeFamily;
        public string TipoTextoCategoria { get; set; } = DefaultTextTypeCategory;
        public string CorTexto { get; set; } = DefaultTextColor;
        public string Fonte { get; set; } = DefaultFont;
        public string AlinhamentoHorizontal { get; set; } = DefaultHorizontalAlignment;
        public double Rotacao { get; set; }
        public bool Visible { get; set; } = true;

        public string Texto
        {
            get => _texto;
            set => _texto = value ?? string.Empty;
        }

        public double LarguraCaixa
        {
            get => _larguraCaixa;
            set => _larguraCaixa = NormalizarDimensao(value, DefaultBoxWidth, MinBoxWidth);
        }

        public double AlturaTexto
        {
            get => _alturaTexto;
            set => _alturaTexto = NormalizarDimensao(value, DefaultTextHeight, MinTextHeight);
        }

        public bool PossuiTipoTexto =>
            !string.IsNullOrWhiteSpace(TipoTextoNome) &&
            !string.IsNullOrWhiteSpace(TipoTextoFamilia) &&
            !string.IsNullOrWhiteSpace(TipoTextoCategoria);

        public void DefinirTipoTexto(string? nomeTipo, string? familia, string? categoria)
        {
            TipoTextoNome = NormalizarTexto(nomeTipo);
            TipoTextoFamilia = NormalizarTexto(familia);
            TipoTextoCategoria = NormalizarTexto(categoria);
        }

        public bool TipoTextoIgual(string? nomeTipo, string? familia, string? categoria)
        {
            return string.Equals(TipoTextoNome, NormalizarTexto(nomeTipo), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TipoTextoFamilia, NormalizarTexto(familia), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TipoTextoCategoria, NormalizarTexto(categoria), StringComparison.OrdinalIgnoreCase);
        }

        public ProjectSheetTemplateText CriarCopia(bool gerarNovoId)
        {
            return new ProjectSheetTemplateText
            {
                Id = gerarNovoId ? Guid.NewGuid() : Id,
                Nome = Nome,
                X = X,
                Y = Y,
                Texto = Texto,
                LarguraCaixa = LarguraCaixa,
                TipoTextoNome = TipoTextoNome,
                TipoTextoFamilia = TipoTextoFamilia,
                TipoTextoCategoria = TipoTextoCategoria,
                CorTexto = CorTexto,
                Fonte = Fonte,
                AlturaTexto = AlturaTexto,
                AlinhamentoHorizontal = AlinhamentoHorizontal,
                Rotacao = Rotacao,
                Visible = Visible
            };
        }

        private static string NormalizarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        }

        private static double NormalizarDimensao(double valor, double fallback, double minimo)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < minimo
                ? fallback
                : valor;
        }
    }
}