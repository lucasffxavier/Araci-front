using System;
using System.Linq;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class TextoAnotativo : ElementoAnotativo
    {
        public const string PARAM_TEXTO = "Texto";
        private const double LARGURA_MINIMA_TEXTO = 180;

        public TextoAnotativo()
        {
            DefinirParametro(new Parameter<string>(PARAM_TEXTO, "Texto"));
        }

        public string Texto
        {
            get => Obter<string>(PARAM_TEXTO);
            set => Definir(PARAM_TEXTO, value ?? string.Empty);
        }

        public TipoTextoAnotativo? TipoTexto => Tipo as TipoTextoAnotativo;

        public string CorTexto => TipoTexto?.CorTexto ?? "#FF000000";
        public double AlturaTexto => TipoTexto?.AlturaTexto ?? 14.0;
        public string Fonte => TipoTexto?.Fonte ?? "Arial";
        public string AlinhamentoHorizontal => TipoTexto?.AlinhamentoHorizontal ?? "Esquerda";

        public double LarguraEstimada
        {
            get
            {
                string[] linhas = ObterLinhas();
                int maiorLinha = linhas.Length == 0 ? 1 : linhas.Max(l => l.Length);
                return Math.Max(LARGURA_MINIMA_TEXTO, maiorLinha * AlturaTexto * 0.58 + 4);
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
    }
}