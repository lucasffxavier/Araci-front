using Araci.Models.Tipos;

namespace Araci.Models
{
    public abstract class ElementoEquipamento
        : Elemento
    {
        // =========================
        // PARÂMETROS BIM
        // =========================

        public const string PARAM_BARRA =
            "Barra";

        public const string PARAM_ALIMENTADOR =
            "Alimentador";

        public const string PARAM_POTENCIA_KW =
            "PotenciaAtivaKW";

        // =========================
        // CONSTRUTOR
        // =========================

        protected ElementoEquipamento()
        {
            DefinirParametro(
                new Parameter<string>(
                    PARAM_NOME,
                    string.Empty));

            DefinirParametro(
                new Parameter<string>(
                    PARAM_BARRA,
                    string.Empty));

            DefinirParametro(
                new Parameter<string>(
                    PARAM_ALIMENTADOR,
                    string.Empty));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_POTENCIA_KW,
                    0));
        }

        // =========================
        // WRAPPERS BIM
        // =========================


        public string Barra
        {
            get => Obter<string>(
                PARAM_BARRA);

            set => Definir(
                PARAM_BARRA,
                value);
        }

        public string Alimentador
        {
            get => Obter<string>(
                PARAM_ALIMENTADOR);

            set => Definir(
                PARAM_ALIMENTADOR,
                value);
        }

        public double PotenciaAtivaKW
        {
            get => Obter<double>(
                PARAM_POTENCIA_KW);

            set => Definir(
                PARAM_POTENCIA_KW,
                value);
        }

        // =========================
        // CLONAGEM
        // =========================

        protected void CopiarEquipamentoPara(
            ElementoEquipamento destino)
        {
            CopiarBasePara(destino);
        }
    }
}