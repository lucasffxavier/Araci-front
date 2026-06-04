namespace Araci.Models.Tipos
{
    public class TipoTextoAnotativo : TipoElemento
    {
        public const string PARAM_FONTE = "Fonte";
        public const string PARAM_ALTURA_TEXTO = "AlturaTexto";
        public const string PARAM_ALINHAMENTO_HORIZONTAL = "AlinhamentoHorizontal";

        public TipoTextoAnotativo()
        {
            NomeTipo = "Texto padrão";
            Familia = "Anotações";
            Categoria = "Textos";
            DefinirParametro(new Parameter<string>(PARAM_FONTE, "Segoe UI"));
            DefinirParametro(new Parameter<double>(PARAM_ALTURA_TEXTO, 14.0));
            DefinirParametro(new Parameter<string>(PARAM_ALINHAMENTO_HORIZONTAL, "Esquerda"));
        }

        public string Fonte
        {
            get => Obter<string>(PARAM_FONTE);
            set => Definir(PARAM_FONTE, string.IsNullOrWhiteSpace(value) ? "Segoe UI" : value.Trim());
        }

        public double AlturaTexto
        {
            get => Obter<double>(PARAM_ALTURA_TEXTO);
            set => Definir(PARAM_ALTURA_TEXTO, value <= 0 ? 14.0 : value);
        }

        public string AlinhamentoHorizontal
        {
            get => Obter<string>(PARAM_ALINHAMENTO_HORIZONTAL);
            set => Definir(PARAM_ALINHAMENTO_HORIZONTAL, NormalizarAlinhamento(value));
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