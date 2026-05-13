using System;
using System.Windows;
using System.Windows.Media;

using Araci.Models;

namespace Araci.ViewModels
{
    public class CaboViewModel
        : ElementoViewModel
    {
        private readonly Cabo _cabo;

        public CaboViewModel(
            Cabo cabo)
            : base(cabo)
        {
            _cabo = cabo
                ?? throw new ArgumentNullException(nameof(cabo));

            if (_cabo.PosicaoX2 == 0 &&
                _cabo.PosicaoY2 == 0)
            {
                _cabo.PosicaoX2 =
                    _cabo.PosicaoX + 120;

                _cabo.PosicaoY2 =
                    _cabo.PosicaoY;
            }

            VisualState.DefinirVisualBase(
                Brushes.Black,
                4);

            AtualizarGeometria();
        }

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

        public string BarraOrigem
        {
            get => _cabo.BarraOrigem;
            set
            {
                if (_cabo.BarraOrigem != value)
                {
                    _cabo.BarraOrigem = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BarraDestino
        {
            get => _cabo.BarraDestino;
            set
            {
                if (_cabo.BarraDestino != value)
                {
                    _cabo.BarraDestino = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TipoCabo
        {
            get => _cabo.TipoCabo;
            set
            {
                if (_cabo.TipoCabo != value)
                {
                    _cabo.TipoCabo = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Comprimento
        {
            get => _cabo.Comprimento;
            set
            {
                if (_cabo.Comprimento != value)
                {
                    _cabo.Comprimento = value;
                    OnPropertyChanged();
                }
            }
        }

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

        protected override double LarguraBase =>
            Math.Max(8, Math.Abs(X2 - X));

        protected override double AlturaBase =>
            Math.Max(8, Math.Abs(Y2 - Y));

        protected override void AtualizarGeometria()
        {
            Geometry.Atualizar(
                Math.Min(X, X2),
                Math.Min(Y, Y2),
                Math.Max(8, Math.Abs(X2 - X)),
                Math.Max(8, Math.Abs(Y2 - Y)));

            NotificarGeometria();

            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }

        public override void Mover(
            Vector delta)
        {
            Transform.X += delta.X;
            Transform.Y += delta.Y;

            _modelo.PosicaoX = Transform.X;
            _modelo.PosicaoY = Transform.Y;

            _cabo.PosicaoX2 += delta.X;
            _cabo.PosicaoY2 += delta.Y;

            AtualizarGeometria();
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
            Transform.X = estado.X;
            Transform.Y = estado.Y;

            _modelo.PosicaoX = estado.X;
            _modelo.PosicaoY = estado.Y;

            _cabo.PosicaoX2 = estado.X2;
            _cabo.PosicaoY2 = estado.Y2;

            AtualizarGeometria();
        }
    }
}