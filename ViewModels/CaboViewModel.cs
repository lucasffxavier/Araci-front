using System;
using System.Windows;

using Araci.Models;

namespace Araci.ViewModels
{
    public class CaboViewModel
        : ElementoViewModel
    {
        private readonly Cabo _cabo;

        public CaboViewModel(
            Cabo cabo)
            : base(cabo ?? throw new ArgumentNullException(nameof(cabo)))
        {
            _cabo = cabo;
        }

        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome
        {
            get => _cabo.Nome;

            set
            {
                if (_cabo.Nome == value)
                    return;

                _cabo.Nome = value;

                OnPropertyChanged();
            }
        }

        // =========================
        // PROPRIEDADES ELÉTRICAS
        // =========================

        public string BarraOrigem
        {
            get => _cabo.BarraOrigem;

            set
            {
                if (_cabo.BarraOrigem == value)
                    return;

                _cabo.BarraOrigem = value;

                OnPropertyChanged();
            }
        }

        public string BarraDestino
        {
            get => _cabo.BarraDestino;

            set
            {
                if (_cabo.BarraDestino == value)
                    return;

                _cabo.BarraDestino = value;

                OnPropertyChanged();
            }
        }

        public double Comprimento
        {
            get => _cabo.Comprimento;

            set
            {
                if (_cabo.Comprimento == value)
                    return;

                _cabo.Comprimento = value;

                OnPropertyChanged();
            }
        }

        public double Resistência
        {
            get => _cabo.Resistencia;

            set
            {
                if (_cabo.Resistencia == value)
                    return;

                _cabo.Resistencia = value;

                OnPropertyChanged();
            }
        }

        public double Reatancia
        {
            get => _cabo.Reatancia;

            set
            {
                if (_cabo.Reatancia == value)
                    return;

                _cabo.Reatancia = value;

                OnPropertyChanged();
            }
        }

        public string TipoCabo
        {
            get => _cabo.TipoCabo;

            set
            {
                if (_cabo.TipoCabo == value)
                    return;

                _cabo.TipoCabo = value;

                OnPropertyChanged();
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
                if (_cabo.PosicaoX2 == value)
                    return;

                _cabo.PosicaoX2 = value;

                AtualizarGeometria();
            }
        }

        public double Y2
        {
            get => _cabo.PosicaoY2;

            set
            {
                if (_cabo.PosicaoY2 == value)
                    return;

                _cabo.PosicaoY2 = value;

                AtualizarGeometria();
            }
        }

        public override double Largura =>
            Math.Max(4, Math.Abs(X2 - X));

        public override double Altura =>
            Math.Max(4, Math.Abs(Y2 - Y));

        public override Rect Bounds
        {
            get
            {
                double minX = Math.Min(X, X2);
                double minY = Math.Min(Y, Y2);

                return new Rect(
                    minX,
                    minY,
                    Largura,
                    Altura);
            }
        }

        public override Point Centro =>
            new Point(
                (X + X2) / 2.0,
                (Y + Y2) / 2.0);

        protected override void AtualizarGeometria()
        {
            base.AtualizarGeometria();

            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }

        public override void Mover(
            Vector delta)
        {
            double novoX1 = X + delta.X;
            double novoY1 = Y + delta.Y;

            double novoX2 = X2 + delta.X;
            double novoY2 = Y2 + delta.Y;

            if (AppServices.Viewport != null)
            {
                double largura =
                    AppServices.Viewport.Largura;

                double altura =
                    AppServices.Viewport.Altura;

                if (novoX1 < 0 || novoX2 < 0)
                    return;

                if (novoY1 < 0 || novoY2 < 0)
                    return;

                if (novoX1 > largura || novoX2 > largura)
                    return;

                if (novoY1 > altura || novoY2 > altura)
                    return;
            }

            X = novoX1;
            Y = novoY1;

            X2 = novoX2;
            Y2 = novoY2;
        }

        public override ElementoEstado CapturarEstado()
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

            if (estado.PossuiSegundoPonto)
            {
                X2 = estado.X2;
                Y2 = estado.Y2;
            }
        }
    }
}