using System.Windows;

namespace Araci.ViewModels.VisualStates
{
    public class ElementoGeometryState
    {
        // =========================
        // DIMENSÕES
        // =========================

        public double Largura
        { get; set; }

        public double Altura
        { get; set; }

        // =========================
        // BOUNDS
        // =========================

        public Rect ObterBounds(
            double x,
            double y)
        {
            return new Rect(
                x,
                y,
                Largura,
                Altura);
        }

        // =========================
        // CENTRO
        // =========================

        public Point ObterCentro(
            double x,
            double y)
        {
            return new Point(
                x + (Largura / 2.0),
                y + (Altura / 2.0));
        }
    }
}