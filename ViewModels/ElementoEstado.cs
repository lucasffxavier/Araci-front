namespace Araci.ViewModels
{
    public class ElementoEstado
    {
        // =========================
        // POSIÇÃO
        // =========================

        public double X { get; }

        public double Y { get; }

        // =========================
        // CABO
        // =========================

        public double? X2 { get; }

        public double? Y2 { get; }

        // =========================
        // CONSTRUTOR
        // =========================

        public ElementoEstado(
            double x,
            double y,
            double? x2 = null,
            double? y2 = null)
        {
            X = x;
            Y = y;

            X2 = x2;
            Y2 = y2;
        }
    }
}