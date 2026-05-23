namespace Araci.Models.Tipos
{
    public class TipoBarra : TipoElemento
    {
        public const string PARAM_CLASSE_TENSAO = "ClasseTensao";
        public const string PARAM_FASES = "Fases";

        private int _numeroConexoes = 6;

        public TipoBarra()
        {
            NomeTipo = "Barra Vertical";
            Familia = "Barras";
            Categoria = "Barras";

            DefinirParametro(new Parameter<string>(PARAM_CLASSE_TENSAO, "13.8"));
            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
        }

        public string ClasseTensao
        {
            get => Obter<string>(PARAM_CLASSE_TENSAO);
            set => Definir(PARAM_CLASSE_TENSAO, value);
        }

        public int Fases
        {
            get => Obter<int>(PARAM_FASES);
            set => Definir(PARAM_FASES, value <= 0 ? 1 : value);
        }

        public double AlturaPadrao { get; set; } = 120;

        public int NumeroConexoes
        {
            get => _numeroConexoes;
            set => _numeroConexoes = value <= 0 ? 1 : value;
        }
    }
}
