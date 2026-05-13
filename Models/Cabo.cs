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
        // TIPO
        // =========================

        public string TipoCabo { get; set; }

        public double Resistencia { get; set; }

        public double Reatancia { get; set; }

        public double Capacitancia { get; set; }

        public double Ampacidade { get; set; }

        public int Fases { get; set; }

        public bool Neutro { get; set; }

        // =========================
        // CONSTRUTOR
        // =========================

        public Cabo()
        {
            Nome = "CB-01";

            BarraOrigem = "BUS-01";

            BarraDestino = "BUS-02";

            Comprimento = 120;

            // =========================
            // PONTO INICIAL
            // =========================

            PosicaoX = 100;

            PosicaoY = 100;

            // =========================
            // PONTO FINAL
            // =========================

            PosicaoX2 = 400;

            PosicaoY2 = 100;

            // =========================
            // TIPO
            // =========================

            TipoCabo = "LC-500MCM";

            Resistencia = 0.12;

            Reatancia = 0.09;

            Capacitancia = 0.001;

            Ampacidade = 520;

            Fases = 3;

            Neutro = true;

            Categoria = "Cabos";

            Familia = "Cabos";
        }
    }
}