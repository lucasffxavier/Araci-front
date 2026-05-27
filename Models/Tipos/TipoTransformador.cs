namespace Araci.Models.Tipos
{
    public class TipoTransformador : TipoElemento
    {
        public const string PARAM_FASES = "Fases";
        public const string PARAM_ENROLAMENTOS = "Enrolamentos";
        public const string PARAM_TENSAO_PRIMARIO_KV = "TensaoPrimarioKV";
        public const string PARAM_TENSAO_SECUNDARIO_KV = "TensaoSecundarioKV";
        public const string PARAM_POTENCIA_KVA = "PotenciaKVA";
        public const string PARAM_R_PERCENTUAL = "RPercentual";
        public const string PARAM_X_PERCENTUAL = "XPercentual";
        public const string PARAM_LIGACAO_PRIMARIO = "LigacaoPrimario";
        public const string PARAM_LIGACAO_SECUNDARIO = "LigacaoSecundario";

        public TipoTransformador()
        {
            NomeTipo = "Transformador 2 Enrolamentos";
            Familia = "Transformadores";
            Categoria = "Transformadores";

            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
            DefinirParametro(new Parameter<int>(PARAM_ENROLAMENTOS, 2));
            DefinirParametro(new Parameter<double>(PARAM_TENSAO_PRIMARIO_KV, 13.8));
            DefinirParametro(new Parameter<double>(PARAM_TENSAO_SECUNDARIO_KV, 0.38));
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_KVA, 500));
            DefinirParametro(new Parameter<double>(PARAM_R_PERCENTUAL, 1));
            DefinirParametro(new Parameter<double>(PARAM_X_PERCENTUAL, 5));
            DefinirParametro(new Parameter<string>(PARAM_LIGACAO_PRIMARIO, "Wye"));
            DefinirParametro(new Parameter<string>(PARAM_LIGACAO_SECUNDARIO, "Wye"));
        }

        public int Fases
        {
            get => Obter<int>(PARAM_FASES);
            set => Definir(PARAM_FASES, value <= 0 ? 1 : value);
        }

        public int Enrolamentos
        {
            get => Obter<int>(PARAM_ENROLAMENTOS);
            set => Definir(PARAM_ENROLAMENTOS, value <= 0 ? 2 : value);
        }

        public double TensaoPrimarioKV
        {
            get => Obter<double>(PARAM_TENSAO_PRIMARIO_KV);
            set => Definir(PARAM_TENSAO_PRIMARIO_KV, value > 0 ? value : 13.8);
        }

        public double TensaoSecundarioKV
        {
            get => Obter<double>(PARAM_TENSAO_SECUNDARIO_KV);
            set => Definir(PARAM_TENSAO_SECUNDARIO_KV, value > 0 ? value : 0.38);
        }

        public double PotenciaKVA
        {
            get => Obter<double>(PARAM_POTENCIA_KVA);
            set => Definir(PARAM_POTENCIA_KVA, value < 0 ? 0 : value);
        }

        public double RPercentual
        {
            get => Obter<double>(PARAM_R_PERCENTUAL);
            set => Definir(PARAM_R_PERCENTUAL, value < 0 ? 0 : value);
        }

        public double XPercentual
        {
            get => Obter<double>(PARAM_X_PERCENTUAL);
            set => Definir(PARAM_X_PERCENTUAL, value < 0 ? 0 : value);
        }

        public string LigacaoPrimario
        {
            get => Obter<string>(PARAM_LIGACAO_PRIMARIO);
            set => Definir(PARAM_LIGACAO_PRIMARIO, string.IsNullOrWhiteSpace(value) ? "Wye" : value);
        }

        public string LigacaoSecundario
        {
            get => Obter<string>(PARAM_LIGACAO_SECUNDARIO);
            set => Definir(PARAM_LIGACAO_SECUNDARIO, string.IsNullOrWhiteSpace(value) ? "Wye" : value);
        }
    }
}
