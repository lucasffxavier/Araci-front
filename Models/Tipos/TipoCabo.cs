namespace Araci.Models.Tipos
{
    public class TipoCabo
        : TipoElemento
    {
        // =========================
        // PARÂMETROS
        // =========================

        public const string PARAM_RESISTENCIA =
            "Resistencia";

        public const string PARAM_REATANCIA =
            "Reatancia";

        public const string PARAM_CAPACITANCIA =
            "Capacitancia";

        public const string PARAM_AMPACIDADE =
            "Ampacidade";

        public const string PARAM_FASES =
            "Fases";

        public const string PARAM_NEUTRO =
            "Neutro";

        // =========================
        // WRAPPERS BIM
        // =========================

        public double Resistencia
        {
            get => Obter<double>(
                PARAM_RESISTENCIA);

            set => Definir(
                PARAM_RESISTENCIA,
                value);
        }

        public double Reatancia
        {
            get => Obter<double>(
                PARAM_REATANCIA);

            set => Definir(
                PARAM_REATANCIA,
                value);
        }

        public double Capacitancia
        {
            get => Obter<double>(
                PARAM_CAPACITANCIA);

            set => Definir(
                PARAM_CAPACITANCIA,
                value);
        }

        public double Ampacidade
        {
            get => Obter<double>(
                PARAM_AMPACIDADE);

            set => Definir(
                PARAM_AMPACIDADE,
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

        public bool Neutro
        {
            get => Obter<bool>(
                PARAM_NEUTRO);

            set => Definir(
                PARAM_NEUTRO,
                value);
        }

        // =========================
        // CONSTRUTOR
        // =========================

        public TipoCabo()
        {
            NomeTipo = "LC-500MCM";

            Familia = "Cabos";

            Categoria = "Cabos";

            DefinirParametro(
                new Parameter<double>(
                    PARAM_RESISTENCIA,
                    0.12));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_REATANCIA,
                    0.09));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_CAPACITANCIA,
                    0.001));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_AMPACIDADE,
                    520));

            DefinirParametro(
                new Parameter<int>(
                    PARAM_FASES,
                    3));

            DefinirParametro(
                new Parameter<bool>(
                    PARAM_NEUTRO,
                    true));
        }
    }
}