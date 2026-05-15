using System.Windows;

namespace Araci.ViewModels.VisualStates
{
    public class ElementoTransform
    {
        // =========================
        // POSIÇÃO EM MUNDO
        // =========================

        public double X { get; set; }

        public double Y { get; set; }

        // =========================
        // CONSTRUTOR
        // =========================

        public ElementoTransform()
        {
        }

        public ElementoTransform(
            double x,
            double y)
        {
            X = x;
            Y = y;
        }

        // =========================
        // MOVIMENTO
        // =========================

        public void Mover(
            Vector delta)
        {
            X += delta.X;
            Y += delta.Y;
        }
    }
}
