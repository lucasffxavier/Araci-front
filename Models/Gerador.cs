using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Gerador : Elemento
    {
        // =========================
        // INSTÂNCIA
        // =========================

        public string Barra { get; set; }

        public string Alimentador { get; set; }

        public double PotenciaAtivaKW { get; set; }

        public double FatorPotencia { get; set; }

        // =========================
        // TIPO FORTE
        // =========================

        public TipoGerador TipoGerador =>
            (TipoGerador)Tipo!;

        // =========================
        // CONSTRUTOR
        // =========================

        public Gerador()
        {
            Nome = "GER-01";

            Barra = "BUS-01";

            Alimentador = "AL-01";

            PotenciaAtivaKW = 5000;

            FatorPotencia = 0.98;

            PosicaoX = 300;
            PosicaoY = 200;
        }

        // =========================
        // CLONAGEM
        // =========================

        public override Elemento Clonar()
        {
            var clone = new Gerador();

            CopiarBasePara(clone);

            clone.Barra = Barra;
            clone.Alimentador = Alimentador;

            clone.PotenciaAtivaKW = PotenciaAtivaKW;
            clone.FatorPotencia = FatorPotencia;

            return clone;
        }
    }
}
