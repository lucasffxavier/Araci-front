using System.Windows;

using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public class SelectionBoxViewModel
        : ViewModelBase
    {
        private bool _visivel;
        private double _x;
        private double _y;
        private double _largura;
        private double _altura;

        public bool Visivel
        {
            get => _visivel;
            set => Set(ref _visivel, value);
        }

        public double X
        {
            get => _x;
            set => Set(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => Set(ref _y, value);
        }

        public double Largura
        {
            get => _largura;
            set => Set(ref _largura, value);
        }

        public double Altura
        {
            get => _altura;
            set => Set(ref _altura, value);
        }

        public Rect Bounds =>
            new(X, Y, Largura, Altura);

        public void Atualizar(
            Point inicio,
            Point fim)
        {
            X = inicio.X < fim.X
                ? inicio.X
                : fim.X;

            Y = inicio.Y < fim.Y
                ? inicio.Y
                : fim.Y;

            Largura =
                System.Math.Abs(fim.X - inicio.X);

            Altura =
                System.Math.Abs(fim.Y - inicio.Y);
        }
    }
}