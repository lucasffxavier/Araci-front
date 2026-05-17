using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Gerador
        : ElementoEquipamento
    {
        // =========================
        // PARÂMETROS
        // =========================

        public const string PARAM_FATOR_POTENCIA =
            "FatorPotencia";

        // =========================
        // TIPO
        // =========================

        public TipoGerador TipoGerador =>
            (TipoGerador)Tipo!;

        // =========================
        // WRAPPERS BIM
        // =========================

        public double FatorPotencia
        {
            get => Obter<double>(
                PARAM_FATOR_POTENCIA);

            set => Definir(
                PARAM_FATOR_POTENCIA,
                value);
        }

        // =========================
        // CONSTRUTOR
        // =========================

        public Gerador()
        {
            Nome = "GER-01";

            Barra = "BUS-01";

            Alimentador = "AL-01";

            PotenciaAtivaKW = 5000;

            DefinirParametro(
                new Parameter<double>(
                    PARAM_FATOR_POTENCIA,
                    0.98));

            PosicaoX = 300;
            PosicaoY = 200;
        }

        // =========================
        // CLONAGEM
        // =========================

        public override Elemento Clonar()
        {
            var clone = new Gerador();

            CopiarEquipamentoPara(clone);

            return clone;
        }
    }
}