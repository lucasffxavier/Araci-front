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
        public const string PARAM_LEADER_ATIVO = "LeaderAtivo";
        public const string PARAM_LEADER_X = "LeaderX";
        public const string PARAM_LEADER_Y = "LeaderY";
        public const double LarguraCaixaPadrao = 200.0;
        public const double LarguraCaixaMinima = 20.0;
        public const double MargemHorizontalCaixa = 8.0;

        public TextoAnotativo()
        {
            DefinirParametro(new Parameter<string>(PARAM_TEXTO, "Texto"));
            DefinirParametro(new Parameter<double>(PARAM_LARGURA_CAIXA, CalcularLarguraNatural("Texto", 14.0)));
            DefinirParametro(new Parameter<bool>(PARAM_LEADER_ATIVO, false));
            DefinirParametro(new Parameter<double>(PARAM_LEADER_X, 0.0));
            DefinirParametro(new Parameter<double>(PARAM_LEADER_Y, 0.0));
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

        public bool LeaderAtivo
        {
            get => Obter<bool>(PARAM_LEADER_ATIVO);
            set => Definir(PARAM_LEADER_ATIVO, value);
        }

        public double LeaderX
        {
            get => Obter<double>(PARAM_LEADER_X);
            set => Definir(PARAM_LEADER_X, NormalizarCoordenada(value));
        }

        public double LeaderY
        {
            get => Obter<double>(PARAM_LEADER_Y);
            set => Definir(PARAM_LEADER_Y, NormalizarCoordenada(value));
        }

        public TipoTextoAnotativo? TipoTexto => Tipo as TipoTextoAnotativo;

        public string CorTexto => TipoTexto?.CorTexto ?? "#FF000000";
        public double AlturaTexto => TipoTexto?.AlturaTexto ?? 14.0;
        public string Fonte => TipoTexto?.Fonte ?? "Arial";
        public string AlinhamentoHorizontal => TipoTexto?.AlinhamentoHorizontal ?? "Esquerda";

        public double LarguraEstimada => Math.Max(LarguraCaixaMinima, LarguraCaixa);
        public double AlturaEstimada => CalcularAlturaEstimada(Texto, LarguraCaixa, AlturaTexto);

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

        public static double CalcularAlturaEstimada(string? texto, double larguraCaixa, double alturaTexto)
        {
            double altura = alturaTexto <= 0 ? 14.0 : alturaTexto;
            int linhas = Math.Max(1, ObterLinhasRenderizadas(texto, larguraCaixa, altura).Count);
            return Math.Max(altura, linhas * altura * 1.25 + 4);
        }

        public override Elemento Clonar()
        {
            var clone = new TextoAnotativo();
            CopiarBasePara(clone);
            clone.LeaderAtivo = LeaderAtivo;
            clone.LeaderX = LeaderX;
            clone.LeaderY = LeaderY;
            return clone;
        }

        private IReadOnlyList<string> ObterLinhasRenderizadas()
        {
            return ObterLinhasRenderizadas(Texto, LarguraCaixa, AlturaTexto);
        }

        private static IReadOnlyList<string> ObterLinhasRenderizadas(string? texto, double larguraCaixa, double alturaTexto)
        {
            string[] linhasManuais = ObterLinhasManuais(texto);
            var linhas = new List<string>();

            foreach (string linha in linhasManuais)
            {
                foreach (string renderizada in QuebrarLinha(linha, larguraCaixa, alturaTexto))
                    linhas.Add(renderizada);
            }

            return linhas.Count == 0 ? new[] { string.Empty } : linhas;
        }

        private static IEnumerable<string> QuebrarLinha(string linha, double larguraCaixa, double alturaTexto)
        {
            string texto = linha ?? string.Empty;
            int caracteresPorLinha = CalcularCaracteresPorLinha(larguraCaixa, alturaTexto);

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
            return CalcularCaracteresPorLinha(LarguraCaixa, AlturaTexto);
        }

        private static int CalcularCaracteresPorLinha(double larguraCaixa, double alturaTexto)
        {
            double larguraUtil = Math.Max(1, NormalizarLargura(larguraCaixa) - MargemHorizontalCaixa);
            double altura = alturaTexto <= 0 ? 14.0 : alturaTexto;
            double larguraMedia = Math.Max(1, altura * 0.58);
            return Math.Max(1, (int)Math.Floor(larguraUtil / larguraMedia));
        }

        private string[] ObterLinhasManuais()
        {
            return ObterLinhasManuais(Texto);
        }

        private static string[] ObterLinhasManuais(string? texto)
        {
            return (texto ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static double NormalizarLargura(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
                return LarguraCaixaPadrao;

            return Math.Max(LarguraCaixaMinima, valor);
        }

        private static double NormalizarCoordenada(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) ? 0.0 : valor;
        }
    }
}
