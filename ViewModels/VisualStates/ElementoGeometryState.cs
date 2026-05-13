using System.Windows;

namespace Araci.ViewModels.VisualStates
{
    public class ElementoGeometryState
    {
        // =========================
        // POSIÇÃO
        // =========================

        public double X
        { get; set; }

        public double Y
        { get; set; }

        // =========================
        // TAMANHO
        // =========================

        public double Largura
        { get; set; }

        public double Altura
        { get; set; }

        // =========================
        // BOUNDS
        // =========================

        public Rect Bounds =>
            new Rect(
                X,
                Y,
                Largura,
                Altura);

        // =========================
        // CENTRO
        // =========================

        public Point Centro =>
            new Point(
                X + (Largura / 2.0),
                Y + (Altura / 2.0));

        // =========================
        // ATUALIZAR
        // =========================

        public void Atualizar(
            double x,
            double y,
            double largura,
            double altura)
        {
            X = x;
            Y = y;

            Largura = largura;
            Altura = altura;
        }
    }
}