namespace Araci.Models
{
    public abstract class Elemento
    {
        // =========================
        // POSIÇÃO
        // =========================

        public double PosicaoX { get; set; }

        public double PosicaoY { get; set; }

        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome { get; set; } = string.Empty;

        public Guid Id { get; set; } = Guid.NewGuid();

        // =========================
        // VISUAL
        // =========================

        public bool Selecionado { get; set; }

        public double Rotacao { get; set; } = 0;

        public double Escala { get; set; } = 1;

        // =========================
        // BIM
        // =========================

        public string Familia { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;
    }
}