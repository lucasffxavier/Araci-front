using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Carga : Elemento
    {
        // =========================
        // INSTÂNCIA
        // =========================

        public string Barra { get; set; }

        public string Alimentador { get; set; }

        public double PotenciaAtivaKW { get; set; }

        public double PotenciaReativaKvar { get; set; }

        // =========================
        // TIPO FORTE
        // =========================

        public TipoCarga TipoCarga =>
            (TipoCarga)Tipo!;

        // =========================
        // CONSTRUTOR
        // =========================

        public Carga()
        {
            Nome = "LOAD-01";

            Barra = "BUS-03";

            Alimentador = "AL-01";

            PotenciaAtivaKW = 1500;
            PotenciaReativaKvar = 450;

            PosicaoX = 500;
            PosicaoY = 250;

            Tipo = new TipoCarga();
        }

        // =========================
        // CLONAGEM
        // =========================

        public override Elemento Clonar()
        {
            var clone = new Carga();

            CopiarBasePara(clone);

            clone.Barra = Barra;
            clone.Alimentador = Alimentador;

            clone.PotenciaAtivaKW = PotenciaAtivaKW;
            clone.PotenciaReativaKvar = PotenciaReativaKvar;

            return clone;
        }
    }
}