using System;
using System.Windows;

using Araci.Models;

namespace Araci.ViewModels
{
    public class CaboViewModel
        : ElementoViewModel
    {
        // =========================
        // MODELO
        // =========================

        private readonly Cabo _cabo;

        // =========================
        // CONSTRUTOR
        // =========================

        public CaboViewModel(
            Cabo cabo)
            : base(cabo)
        {
            _cabo = cabo;
        }

        // =========================
        // DIMENSÕES
        // =========================

        public override double Largura =>
            Math.Abs(X2 - X);

        public override double Altura =>
            Math.Abs(Y2 - Y);

        // =========================
        // BOUNDS
        // =========================

        public override Rect Bounds
        {
            get
            {
                double minX =
                    Math.Min(X, X2);

                double minY =
                    Math.Min(Y, Y2);

                double largura =
                    Math.Abs(X2 - X);

                double altura =
                    Math.Abs(Y2 - Y);

                return new Rect(
                    minX,
                    minY,
                    largura,
                    altura);
            }
        }

        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome
        {
            get => _cabo.Nome;

            set
            {
                if (_cabo.Nome != value)
                {
                    _cabo.Nome = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // GEOMETRIA
        // =========================

        public double X2
        {
            get => _cabo.PosicaoX2;

            set
            {
                if (_cabo.PosicaoX2 != value)
                {
                    _cabo.PosicaoX2 = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Bounds));
                }
            }
        }

        public double Y2
        {
            get => _cabo.PosicaoY2;

            set
            {
                if (_cabo.PosicaoY2 != value)
                {
                    _cabo.PosicaoY2 = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Bounds));
                }
            }
        }

        // =========================
        // MOVER
        // =========================

        public override void Mover(
            Vector delta)
        {
            X += delta.X;
            Y += delta.Y;

            X2 += delta.X;
            Y2 += delta.Y;
        }

        // =========================
        // ESTADO
        // =========================

        public override ElementoEstado
            CapturarEstado()
        {
            return new ElementoEstado(
                X,
                Y,
                X2,
                Y2);
        }

        public override void AplicarEstado(
            ElementoEstado estado)
        {
            X = estado.X;
            Y = estado.Y;

            if (estado.X2.HasValue)
            {
                X2 = estado.X2.Value;
            }

            if (estado.Y2.HasValue)
            {
                Y2 = estado.Y2.Value;
            }
        }
    }
}