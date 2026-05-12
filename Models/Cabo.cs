namespace Araci.Models
{
    public class Cabo : Elemento
    {
        // =========================
        // INSTÂNCIA
        // =========================

        public string Barra1 { get; set; }

        public string Barra2 { get; set; }

        public double Comprimento { get; set; }

        // =========================
        // PONTO FINAL
        // =========================

        public double PosicaoX2 { get; set; }

        public double PosicaoY2 { get; set; }

        // =========================
        // TIPO
        // =========================

        public string LineCode { get; set; }

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

            Barra1 = "BUS-01";

            Barra2 = "BUS-02";

            Comprimento = 120;

            // PONTO INICIAL

            PosicaoX = 100;

            PosicaoY = 100;

            // PONTO FINAL

            PosicaoX2 = 400;

            PosicaoY2 = 100;

            LineCode = "LC-500MCM";

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