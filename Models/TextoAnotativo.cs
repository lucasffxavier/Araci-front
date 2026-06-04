using System;
using System.Linq;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class TextoAnotativo : ElementoAnotativo
    {
        public const string PARAM_TEXTO = "Texto";
        public const string PARAM_COR_TEXTO = "CorTexto";
        public const string PARAM_ALTURA_TEXTO = "AlturaTexto";
        public const string PARAM_FONTE = "Fonte";
        public const string PARAM_ALINHAMENTO_HORIZONTAL = "AlinhamentoHorizontal";

        public TextoAnotativo()
        {
            DefinirParametro(new Parameter<string>(PARAM_TEXTO, "Texto"));
            DefinirParametro(new Parameter<string>(PARAM_COR_TEXTO, "#FF000000"));
            DefinirParametro(new Parameter<double>(PARAM_ALTURA_TEXTO, 14.0));
            DefinirParametro(new Parameter<string>(PARAM_FONTE, "Segoe UI"));
            DefinirParametro(new Parameter<string>(PARAM_ALINHAMENTO_HORIZONTAL, "Esquerda"));
        }

        public string Texto
        {
            get => Obter<string>(PARAM_TEXTO);
            set => Definir(PARAM_TEXTO, value ?? string.Empty);
        }

        public string CorTexto
        {
            get => Obter<string>(PARAM_COR_TEXTO);
            set => Definir(PARAM_COR_TEXTO, string.IsNullOrWhiteSpace(value) ? "#FF000000" : value.Trim());
        }

        public double AlturaTexto
        {
            get => Obter<double>(PARAM_ALTURA_TEXTO);
            set => Definir(PARAM_ALTURA_TEXTO, value <= 0 ? 14.0 : value);
        }

        public string Fonte
        {
            get => Obter<string>(PARAM_FONTE);
            set => Definir(PARAM_FONTE, string.IsNullOrWhiteSpace(value) ? "Segoe UI" : value.Trim());
        }

        public string AlinhamentoHorizontal
        {
            get => Obter<string>(PARAM_ALINHAMENTO_HORIZONTAL);
            set => Definir(PARAM_ALINHAMENTO_HORIZONTAL, NormalizarAlinhamento(value));
        }

        public TipoTextoAnotativo? TipoTexto => Tipo as TipoTextoAnotativo;

        public double LarguraEstimada
        {
            get
            {
                string[] linhas = ObterLinhas();
                int maiorLinha = linhas.Length == 0 ? 1 : linhas.Max(l => l.Length);
                return Math.Max(20, maiorLinha * AlturaTexto * 0.58 + 4);
            }
        }

        public double AlturaEstimada
        {
            get
            {
                int linhas = Math.Max(1, ObterLinhas().Length);
                return Math.Max(AlturaTexto, linhas * AlturaTexto * 1.25);
            }
        }

        public void AplicarTipoSeNecessario()
        {
            if (TipoTexto == null)
                return;

            Fonte = TipoTexto.Fonte;
            AlturaTexto = TipoTexto.AlturaTexto;
            AlinhamentoHorizontal = TipoTexto.AlinhamentoHorizontal;
        }

        public override Elemento Clonar()
        {
            var clone = new TextoAnotativo();
            CopiarBasePara(clone);
            return clone;
        }

        private string[] ObterLinhas()
        {
            return (Texto ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static string NormalizarAlinhamento(string? valor)
        {
            return valor switch
            {
                "Centro" => "Centro",
                "Direita" => "Direita",
                _ => "Esquerda"
            };
        }
    }
}