using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetTemplateText
    {
        public const string DefaultText = "Texto";
        public const double DefaultBoxWidth = 200.0;
        public const double MinBoxWidth = 20.0;
        public const double MargemHorizontalCaixa = 8.0;
        public const string DefaultTextTypeName = "Texto padrão";
        public const string DefaultTextTypeFamily = "Anotações";
        public const string DefaultTextTypeCategory = "Textos";
        public const string DefaultTextColor = "#FF000000";
        public const string DefaultFont = "Arial";
        public const double DefaultTextHeight = 14.0;
        public const double MinTextHeight = 0.0001;
        public const string DefaultHorizontalAlignment = "Esquerda";

        private const double FatorLarguraMediaCaractere = 0.58;

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
        public bool LeaderAtivo { get; set; }
        public double LeaderX { get; set; }
        public double LeaderY { get; set; }
        public bool LeaderComCotovelo { get; set; }
        public double LeaderCotoveloX { get; set; }
        public double LeaderCotoveloY { get; set; }
        public bool LeaderCotoveloManual { get; set; }
        public bool Visible { get; set; } = true;

        public string Texto
        {
            get => _texto;
            set => _texto = value ?? string.Empty;
        }

        public double LarguraCaixa
        {
            get => _larguraCaixa;
            set => _larguraCaixa = NormalizarLargura(value);
        }

        public double AlturaTexto
        {
            get => _alturaTexto;
            set => _alturaTexto = NormalizarAlturaTexto(value);
        }

        public bool PossuiTipoTexto =>
            !string.IsNullOrWhiteSpace(TipoTextoNome) &&
            !string.IsNullOrWhiteSpace(TipoTextoFamilia) &&
            !string.IsNullOrWhiteSpace(TipoTextoCategoria);

        public bool PossuiLeaderPointValido =>
            ValorFinito(LeaderX) &&
            ValorFinito(LeaderY) &&
            (Math.Abs(LeaderX) > 0.000001 || Math.Abs(LeaderY) > 0.000001);

        public bool PossuiLeaderCotoveloPointValido =>
            ValorFinito(LeaderCotoveloX) &&
            ValorFinito(LeaderCotoveloY) &&
            (Math.Abs(LeaderCotoveloX) > 0.000001 || Math.Abs(LeaderCotoveloY) > 0.000001);

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
                LeaderAtivo = LeaderAtivo,
                LeaderX = LeaderX,
                LeaderY = LeaderY,
                LeaderComCotovelo = LeaderComCotovelo,
                LeaderCotoveloX = LeaderCotoveloX,
                LeaderCotoveloY = LeaderCotoveloY,
                LeaderCotoveloManual = LeaderCotoveloManual,
                Visible = Visible
            };
        }

        public static double CalcularLarguraNatural(string? texto, double alturaTexto)
        {
            string[] linhas = ObterLinhasManuais(texto);
            int maiorLinha = 1;

            foreach (string linha in linhas)
                maiorLinha = Math.Max(maiorLinha, linha.Length);

            double altura = NormalizarAlturaTexto(alturaTexto);
            double largura = maiorLinha * altura * FatorLarguraMediaCaractere + MargemHorizontalCaixa;
            return NormalizarLargura(largura);
        }

        public static double NormalizarLargura(double valor)
        {
            return NormalizarDimensao(valor, DefaultBoxWidth, MinBoxWidth);
        }

        public static double NormalizarAlturaTexto(double valor)
        {
            return NormalizarDimensao(valor, DefaultTextHeight, MinTextHeight);
        }

        private static string[] ObterLinhasManuais(string? texto)
        {
            return (texto ?? string.Empty)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Split('\n');
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

        private static bool ValorFinito(double valor)
        {
            return !double.IsNaN(valor) && !double.IsInfinity(valor);
        }
    }
}