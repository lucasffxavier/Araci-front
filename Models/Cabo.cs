using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Cabo : Elemento
    {
        // =========================
        // INSTÂNCIA
        // =========================

        public string BarraOrigem { get; set; }

        public string BarraDestino { get; set; }

        public double Comprimento { get; set; }

        // =========================
        // GEOMETRIA
        // =========================

        public double PosicaoX2 { get; set; }

        public double PosicaoY2 { get; set; }

        // =========================
        // TIPO FORTE
        // =========================

        public TipoCabo TipoCabo =>
            (TipoCabo)Tipo!;

        // =========================
        // CONSTRUTOR
        // =========================

        public Cabo()
        {
            Nome = "CB-01";

            BarraOrigem = "BUS-01";
            BarraDestino = "BUS-02";

            Comprimento = 120;

            PosicaoX = 100;
            PosicaoY = 100;

            PosicaoX2 = 400;
            PosicaoY2 = 100;

            Tipo = new TipoCabo();
        }

        // =========================
        // CLONAGEM
        // =========================

        public override Elemento Clonar()
        {
            var clone = new Cabo();

            CopiarBasePara(clone);

            clone.BarraOrigem = BarraOrigem;
            clone.BarraDestino = BarraDestino;

            clone.Comprimento = Comprimento;

            clone.PosicaoX2 = PosicaoX2;
            clone.PosicaoY2 = PosicaoY2;

            return clone;
        }
    }
}