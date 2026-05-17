namespace Araci.Models.Tipos
{
    public class TipoGerador
        : TipoElemento
    {

        public const string PARAM_FABRICANTE =
            "Fabricante";

        public const string PARAM_MODELO =
            "Modelo";

        public const string PARAM_POTENCIA =
            "PotenciaNominalKW";

        public const string PARAM_TENSAO =
            "TensaoKV";

        public const string PARAM_FASES =
            "Fases";

        public string CategoriaGerador
        {
            get => Obter<string>(
                PARAM_CATEGORIA);

            set => Definir(
                PARAM_CATEGORIA,
                value);
        }

        public string Fabricante
        {
            get => Obter<string>(
                PARAM_FABRICANTE);

            set => Definir(
                PARAM_FABRICANTE,
                value);
        }

        public string Modelo
        {
            get => Obter<string>(
                PARAM_MODELO);

            set => Definir(
                PARAM_MODELO,
                value);
        }

        public double PotenciaNominalKW
        {
            get => Obter<double>(
                PARAM_POTENCIA);

            set => Definir(
                PARAM_POTENCIA,
                value);
        }

        public double TensaoKV
        {
            get => Obter<double>(
                PARAM_TENSAO);

            set => Definir(
                PARAM_TENSAO,
                value);
        }

        public int Fases
        {
            get => Obter<int>(
                PARAM_FASES);

            set => Definir(
                PARAM_FASES,
                value);
        }

        public TipoGerador()
        {
            NomeTipo = "Gerador Eólico";

            Familia = "Geradores";

            Categoria = "Geradores";

            DefinirParametro(
                new Parameter<string>(
                    PARAM_CATEGORIA,
                    "Eólico"));

            DefinirParametro(
                new Parameter<string>(
                    PARAM_FABRICANTE,
                    "WEG"));

            DefinirParametro(
                new Parameter<string>(
                    PARAM_MODELO,
                    "WTG-5MW"));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_POTENCIA,
                    5000));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_TENSAO,
                    34.5));

            DefinirParametro(
                new Parameter<int>(
                    PARAM_FASES,
                    3));
        }
    }
}