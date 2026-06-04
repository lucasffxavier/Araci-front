namespace Araci.Models.Tipos
{
    public class TipoTextoAnotativo : TipoElemento
    {
        public const string PARAM_COR_TEXTO = "CorTexto";
        public const string PARAM_FONTE = "Fonte";
        public const string PARAM_ALTURA_TEXTO = "AlturaTexto";
        public const string PARAM_ALINHAMENTO_HORIZONTAL = "AlinhamentoHorizontal";

        public TipoTextoAnotativo()
        {
            NomeTipo = "Texto padrão";
            Familia = "Anotações";
            Categoria = "Textos";
            DefinirParametro(new Parameter<string>(PARAM_COR_TEXTO, "#FF000000"));
            DefinirParametro(new Parameter<string>(PARAM_FONTE, "Arial"));
            DefinirParametro(new Parameter<double>(PARAM_ALTURA_TEXTO, 14.0));
            DefinirParametro(new Parameter<string>(PARAM_ALINHAMENTO_HORIZONTAL, "Esquerda"));
        }

        public string CorTexto
        {
            get => Obter<string>(PARAM_COR_TEXTO);
            set => Definir(PARAM_COR_TEXTO, string.IsNullOrWhiteSpace(value) ? "#FF000000" : value.Trim());
        }

        public string Fonte
        {
            get => Obter<string>(PARAM_FONTE);
            set => Definir(PARAM_FONTE, NormalizarFonte(value));
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

        private static string NormalizarFonte(string? valor)
        {
            return valor switch
            {
                "Arial" => "Arial",
                "Arial Narrow" => "Arial Narrow",
                "Calibri" => "Calibri",
                "Segoe UI" => "Segoe UI",
                "Courier New" => "Courier New",
                "Times New Roman" => "Times New Roman",
                "ISOCP" => "ISOCP",
                "ISOCPEUR" => "ISOCPEUR",
                "Romans" => "Romans",
                "Simplex" => "Simplex",
                _ => "Arial"
            };
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