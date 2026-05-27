namespace Araci.Models.Tipos
{
    public class TipoTransformador : TipoElemento
    {
        public const string PARAM_FASES = "Fases";
        public const string PARAM_ENROLAMENTOS = "Enrolamentos";
        public const string PARAM_TENSAO_PRIMARIO_KV = "TensaoPrimarioKV";
        public const string PARAM_TENSAO_SECUNDARIO_KV = "TensaoSecundarioKV";
        public const string PARAM_POTENCIA_KVA = "PotenciaKVA";

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
    }
}
