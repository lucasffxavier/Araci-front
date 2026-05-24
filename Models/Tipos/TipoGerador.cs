namespace Araci.Models.Tipos
{
    public class TipoGerador : TipoElemento
    {
        public const string PARAM_FASES = "Fases";
        public const string PARAM_TENSAO_KV = "TensaoKV";
        public const string PARAM_MODELO_FONTE = "ModeloFonte";
        public const string PARAM_FATOR_POTENCIA = "FatorPotencia";

        public TipoGerador()
        {
            NomeTipo = "Gerador Eolico";
            Familia = "Geradores";
            Categoria = "Geradores";

            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
            DefinirParametro(new Parameter<double>(PARAM_TENSAO_KV, 12.47));
            DefinirParametro(new Parameter<int>(PARAM_MODELO_FONTE, 1));
            DefinirParametro(new Parameter<double>(PARAM_FATOR_POTENCIA, 0.98));
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

        public int ModeloFonte
        {
            get => Obter<int>(PARAM_MODELO_FONTE);
            set => Definir(PARAM_MODELO_FONTE, value < 0 ? 0 : value);
        }

        public double FatorPotencia
        {
            get => Obter<double>(PARAM_FATOR_POTENCIA);
            set => Definir(PARAM_FATOR_POTENCIA, value);
        }
    }
}
