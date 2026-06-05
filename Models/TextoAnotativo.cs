using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class TextoAnotativo : ElementoAnotativo
    {
        public const string PARAM_TEXTO = "Texto";
        public const string PARAM_LARGURA_CAIXA = "LarguraCaixa";
        public const double LarguraCaixaPadrao = 200.0;
        public const double LarguraCaixaMinima = 20.0;
        public const double MargemHorizontalCaixa = 8.0;

        public TextoAnotativo()
        {
            DefinirParametro(new Parameter<string>(PARAM_TEXTO, "Texto"));
            DefinirParametro(new Parameter<double>(PARAM_LARGURA_CAIXA, CalcularLarguraNatural("Texto", 14.0)));
        }

        public string Texto
        {
            get => Obter<string>(PARAM_TEXTO);
            set => Definir(PARAM_TEXTO, value ?? string.Empty);
        }

        public double LarguraCaixa
        {
            get => Obter<double>(PARAM_LARGURA_CAIXA);
            set => Definir(PARAM_LARGURA_CAIXA, NormalizarLargura(value));
        }

        public TipoTextoAnotativo? TipoTexto => Tipo as TipoTextoAnotativo;

        public string CorTexto => TipoTexto?.CorTexto ?? "#FF000000";
        public double AlturaTexto => TipoTexto?.AlturaTexto ?? 14.0;
        public string Fonte => TipoTexto?.Fonte ?? "Arial";
        public string AlinhamentoHorizontal => TipoTexto?.AlinhamentoHorizontal ?? "Esquerda";

        public double LarguraEstimada => Math.Max(LarguraCaixaMinima, LarguraCaixa);

        public double AlturaEstimada
        {
            get
            {
                int linhas = Math.Max(1, ObterLinhasRenderizadas().Count);
                return Math.Max(AlturaTexto, linhas * AlturaTexto * 1.25 + 4);
            }
        }

        public void AjustarLarguraAoConteudo()
        {
            LarguraCaixa = CalcularLarguraNatural(Texto, AlturaTexto);
        }

        public static double CalcularLarguraNatural(string? texto, double alturaTexto)
        {
            string normalizado = texto ?? string.Empty;
            string[] linhas = normalizado.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int maiorLinha = Math.Max(1, linhas.Length == 0 ? 1 : linhas.Max(l => l.Length));
            double altura = alturaTexto <= 0 ? 14.0 : alturaTexto;
            double largura = maiorLinha * altura * 0.58 + MargemHorizontalCaixa;
            return NormalizarLargura(largura);
        }

        public override Elemento Clonar()
        {
            var clone = new TextoAnotativo();
            CopiarBasePara(clone);
            return clone;
        }

        private IReadOnlyList<string> ObterLinhasRenderizadas()
        {
            string[] linhasManuais = ObterLinhasManuais();
            var linhas = new List<string>();

            foreach (string linha in linhasManuais)
            {
                foreach (string renderizada in QuebrarLinha(linha))
                    linhas.Add(renderizada);
            }

            return linhas.Count == 0 ? new[] { string.Empty } : linhas;
        }

        private IEnumerable<string> QuebrarLinha(string linha)
        {
            string texto = linha ?? string.Empty;
            int caracteresPorLinha = CalcularCaracteresPorLinha();

            if (texto.Length <= caracteresPorLinha)
            {
                yield return texto;
                yield break;
            }

            int inicio = 0;

            while (inicio < texto.Length)
            {
                int restante = texto.Length - inicio;

                if (restante <= caracteresPorLinha)
                {
                    yield return texto[inicio..];
                    yield break;
                }

                int limite = inicio + caracteresPorLinha;
                int quebra = texto.LastIndexOf(' ', limite, caracteresPorLinha);

                if (quebra <= inicio)
                    quebra = limite;

                string trecho = texto[inicio..quebra].TrimEnd();

                if (trecho.Length == 0)
                    trecho = texto.Substring(inicio, Math.Min(caracteresPorLinha, restante));

                yield return trecho;

                inicio = quebra;

                while (inicio < texto.Length && texto[inicio] == ' ')
                    inicio++;
            }
        }

        private int CalcularCaracteresPorLinha()
        {
            double larguraUtil = Math.Max(1, LarguraEstimada - MargemHorizontalCaixa);
            double larguraMedia = Math.Max(1, AlturaTexto * 0.58);
            return Math.Max(1, (int)Math.Floor(larguraUtil / larguraMedia));
        }

        private string[] ObterLinhasManuais()
        {
            return (Texto ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static double NormalizarLargura(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
                return LarguraCaixaPadrao;

            return Math.Max(LarguraCaixaMinima, valor);
        }
    }
}
