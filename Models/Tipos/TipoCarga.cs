namespace Araci.Models.Tipos
{
    public class TipoCarga : TipoElemento
    {
        public const string PARAM_MODELO_CARGA = "ModeloCarga";
        public const string PARAM_CONEXAO = "Conexao";
        public const string PARAM_TENSAO = "Tensao";
        public const string PARAM_FASES = "Fases";
        public const string PARAM_FATOR_POTENCIA = "FatorPotencia";

        public TipoCarga()
        {
            NomeTipo = "Carga MT";
            Familia = "Cargas";
            Categoria = "Cargas";

            DefinirParametro(new Parameter<int>(PARAM_MODELO_CARGA, 1));
            DefinirParametro(new Parameter<string>(PARAM_CONEXAO, "Wye"));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO, "13.8"));
            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
            DefinirParametro(new Parameter<double>(PARAM_FATOR_POTENCIA, 0.96));
        }

        public int ModeloCarga
        {
            get => Obter<int>(PARAM_MODELO_CARGA);
            set => Definir(PARAM_MODELO_CARGA, value < 0 ? 0 : value);
        }

        public string Conexao
        {
            get => Obter<string>(PARAM_CONEXAO);
            set => Definir(PARAM_CONEXAO, value);
        }

        public string Tensao
        {
            get => Obter<string>(PARAM_TENSAO);
            set => Definir(PARAM_TENSAO, value);
        }

        public int Fases
        {
            get => Obter<int>(PARAM_FASES);
            set => Definir(PARAM_FASES, value <= 0 ? 1 : value);
        }

        public double FatorPotencia
        {
            get => Obter<double>(PARAM_FATOR_POTENCIA);
            set => Definir(PARAM_FATOR_POTENCIA, value);
        }
    }
}
