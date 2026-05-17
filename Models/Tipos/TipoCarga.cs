namespace Araci.Models.Tipos
{
    public class TipoCarga
        : TipoElemento
    {
        public const string PARAM_MODELO_CARGA =
            "ModeloCarga";

        public const string PARAM_CONEXAO =
            "Conexao";

        public const string PARAM_TENSAO_KV =
            "TensaoKV";

        public const string PARAM_FASES =
            "Fases";

        public const string PARAM_FP =
            "FatorPotencia";

        public string ModeloCarga
        {
            get => Obter<string>(
                PARAM_MODELO_CARGA);

            set => Definir(
                PARAM_MODELO_CARGA,
                value);
        }

        public string Conexao
        {
            get => Obter<string>(
                PARAM_CONEXAO);

            set => Definir(
                PARAM_CONEXAO,
                value);
        }

        public double TensaoKV
        {
            get => Obter<double>(
                PARAM_TENSAO_KV);

            set => Definir(
                PARAM_TENSAO_KV,
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

        public double FatorPotencia
        {
            get => Obter<double>(
                PARAM_FP);

            set => Definir(
                PARAM_FP,
                value);
        }

        public TipoCarga()
        {
            NomeTipo = "Carga MT";

            Familia = "Cargas";

            Categoria = "Cargas";

            DefinirParametro(
                new Parameter<string>(
                    PARAM_MODELO_CARGA,
                    "Potência Constante"));

            DefinirParametro(
                new Parameter<string>(
                    PARAM_CONEXAO,
                    "Wye"));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_TENSAO_KV,
                    34.5));

            DefinirParametro(
                new Parameter<int>(
                    PARAM_FASES,
                    3));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_FP,
                    0.96));
        }
    }
}