using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Carga
        : ElementoEquipamento
    {
        public double PotenciaReativaKvar { get; set; }

        public TipoCarga TipoCarga =>
            (TipoCarga)Tipo!;

        public Carga()
        {
            Nome = "LOAD-01";

            Barra = "BUS-03";

            Alimentador = "AL-01";

            PotenciaAtivaKW = 1500;
            PotenciaReativaKvar = 450;

            PosicaoX = 500;
            PosicaoY = 250;
        }

        public override Elemento Clonar()
        {
            var clone = new Carga();

            CopiarEquipamentoPara(clone);

            clone.PotenciaReativaKvar =
                PotenciaReativaKvar;

            return clone;
        }
    }
}