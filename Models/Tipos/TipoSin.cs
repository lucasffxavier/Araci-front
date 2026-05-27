namespace Araci.Models.Tipos
{
    public class TipoSin : TipoElemento
    {
        public const string PARAM_FASES = "Fases";
        public const string PARAM_TENSAO_KV = "TensaoKV";
        public const string PARAM_POTENCIA_CURTO_MVA = "PotenciaCurtoMVA";
        public const string PARAM_RELACAO_XR = "RelacaoXR";

        public TipoSin()
        {
            NomeTipo = "Rede Externa";
            Familia = "Fontes";
            Categoria = "SIN";

            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
            DefinirParametro(new Parameter<double>(PARAM_TENSAO_KV, 12.47));
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_CURTO_MVA, 500));
            DefinirParametro(new Parameter<double>(PARAM_RELACAO_XR, 10));
        }

        public int Fases
        {
            get => Obter<int>(PARAM_FASES);
            set => Definir(PARAM_FASES, value <= 0 ? 1 : value);
        }

        public double TensaoKV
        {
            get => Obter<double>(PARAM_TENSAO_KV);
            set => Definir(PARAM_TENSAO_KV, value > 0 ? value : 12.47);
        }

        public double PotenciaCurtoMVA
        {
            get => Obter<double>(PARAM_POTENCIA_CURTO_MVA);
            set => Definir(PARAM_POTENCIA_CURTO_MVA, value < 0 ? 0 : value);
        }

        public double RelacaoXR
        {
            get => Obter<double>(PARAM_RELACAO_XR);
            set => Definir(PARAM_RELACAO_XR, value < 0 ? 0 : value);
        }
    }
}
