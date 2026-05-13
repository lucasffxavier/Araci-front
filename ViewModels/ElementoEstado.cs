namespace Araci.ViewModels
{
    public sealed class ElementoEstado
    {
        // =========================
        // POSIÇÃO
        // =========================

        public double X { get; }

        public double Y { get; }

        // =========================
        // CABOS
        // =========================

        public double X2 { get; }

        public double Y2 { get; }

        // =========================
        // FLAGS
        // =========================

        public bool PossuiSegundoPonto { get; }

        // =========================
        // CONSTRUTOR
        // =========================

        public ElementoEstado(
            double x,
            double y)
        {
            X = x;
            Y = y;

            X2 = 0;
            Y2 = 0;

            PossuiSegundoPonto = false;
        }

        public ElementoEstado(
            double x,
            double y,
            double x2,
            double y2)
        {
            X = x;
            Y = y;

            X2 = x2;
            Y2 = y2;

            PossuiSegundoPonto = true;
        }
    }
}