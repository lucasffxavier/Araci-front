namespace Araci.Models.Tipos
{
    public class TipoTextoAnotativo : TipoElemento
    {
        public const string PARAM_COR_TEXTO = "CorTexto";
        public const string PARAM_FONTE = "Fonte";
        public const string PARAM_ALTURA_TEXTO = "AlturaTexto";
        public const string PARAM_ALINHAMENTO_HORIZONTAL = "AlinhamentoHorizontal";
        public const string PARAM_LEADER_ESTILO_SETA = "LeaderEstiloSeta";
        public const string PARAM_LEADER_COR = "LeaderCor";
        public const string PARAM_LEADER_ESPESSURA = "LeaderEspessura";
        public const string PARAM_LEADER_TAMANHO_SETA = "LeaderTamanhoSeta";

        public TipoTextoAnotativo()
        {
            NomeTipo = "Texto padrão";
            Familia = "Anotações";
            Categoria = "Textos";
            DefinirParametro(new Parameter<string>(PARAM_COR_TEXTO, "#FF000000"));
            DefinirParametro(new Parameter<string>(PARAM_FONTE, "Arial"));
            DefinirParametro(new Parameter<double>(PARAM_ALTURA_TEXTO, 14.0));
            DefinirParametro(new Parameter<string>(PARAM_ALINHAMENTO_HORIZONTAL, "Esquerda"));
            DefinirParametro(new Parameter<string>(PARAM_LEADER_ESTILO_SETA, "Seta preenchida"));
            DefinirParametro(new Parameter<string>(PARAM_LEADER_COR, "#FF000000"));
            DefinirParametro(new Parameter<double>(PARAM_LEADER_ESPESSURA, 1.2));
            DefinirParametro(new Parameter<double>(PARAM_LEADER_TAMANHO_SETA, 10.0));
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

        public string LeaderEstiloSeta
        {
            get => Obter<string>(PARAM_LEADER_ESTILO_SETA);
            set => Definir(PARAM_LEADER_ESTILO_SETA, NormalizarEstiloSetaLeader(value));
        }

        public string LeaderCor
        {
            get => Obter<string>(PARAM_LEADER_COR);
            set => Definir(PARAM_LEADER_COR, string.IsNullOrWhiteSpace(value) ? "#FF000000" : value.Trim());
        }

        public double LeaderEspessura
        {
            get => Obter<double>(PARAM_LEADER_ESPESSURA);
            set => Definir(PARAM_LEADER_ESPESSURA, value <= 0 ? 1.2 : value);
        }

        public double LeaderTamanhoSeta
        {
            get => Obter<double>(PARAM_LEADER_TAMANHO_SETA);
            set => Definir(PARAM_LEADER_TAMANHO_SETA, value <= 0 ? 10.0 : value);
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

        private static string NormalizarEstiloSetaLeader(string? valor)
        {
            return valor switch
            {
                "Sem seta" => "Sem seta",
                "Seta aberta" => "Seta aberta",
                _ => "Seta preenchida"
            };
        }
    }
}