using System.Windows;

namespace Araci.ViewModels.VisualStates
{
    public class ElementoGeometryState
    {
        // =========================
        // POSIÇÃO GLOBAL
        // =========================

        public double X
        { get; private set; }

        public double Y
        { get; private set; }

        // =========================
        // TAMANHO
        // =========================

        public double Largura
        { get; private set; }

        public double Altura
        { get; private set; }

        // =========================
        // GEOMETRIA LOCAL
        // =========================

        public Point PontoLocalInicial
        { get; private set; }

        public Point PontoLocalFinal
        { get; private set; }

        // =========================
        // BOUNDS
        // =========================

        public Rect Bounds =>
            new Rect(
                X,
                Y,
                Largura,
                Altura);

        public Point Centro =>
            new Point(
                X + (Largura / 2.0),
                Y + (Altura / 2.0));

        // =========================
        // ELEMENTO PADRÃO
        // =========================

        public void AtualizarRetangulo(
            double x,
            double y,
            double largura,
            double altura)
        {
            X = x;
            Y = y;

            Largura = largura;
            Altura = altura;

            PontoLocalInicial =
                new Point(0, 0);

            PontoLocalFinal =
                new Point(largura, altura);
        }

        // =========================
        // CABO / LINHA
        // =========================

        public void AtualizarLinha(
            double x1,
            double y1,
            double x2,
            double y2)
        {
            double minX =
                System.Math.Min(x1, x2);

            double minY =
                System.Math.Min(y1, y2);

            double largura =
                System.Math.Max(
                    8,
                    System.Math.Abs(x2 - x1));

            double altura =
                System.Math.Max(
                    8,
                    System.Math.Abs(y2 - y1));

            X = minX;
            Y = minY;

            Largura = largura;
            Altura = altura;

            PontoLocalInicial =
                new Point(
                    x1 - minX,
                    y1 - minY);

            PontoLocalFinal =
                new Point(
                    x2 - minX,
                    y2 - minY);
        }
    }
}