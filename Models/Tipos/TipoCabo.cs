namespace Araci.Models.Tipos
{
    public class TipoCabo : TipoElemento
    {
        // =========================
        // ELÉTRICO
        // =========================

        public double Resistencia { get; set; }

        public double Reatancia { get; set; }

        public double Capacitancia { get; set; }

        public double Ampacidade { get; set; }

        public int Fases { get; set; }

        public bool Neutro { get; set; }

        // =========================
        // CONSTRUTOR
        // =========================

        public TipoCabo()
        {
            NomeTipo = "LC-500MCM";

            Familia = "Cabos";
            Categoria = "Cabos";

            Resistencia = 0.12;
            Reatancia = 0.09;
            Capacitancia = 0.001;

            Ampacidade = 520;

            Fases = 3;

            Neutro = true;
        }
    }
}