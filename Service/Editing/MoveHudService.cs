using System.ComponentModel;
using System;
using System.Windows;
using Araci.Services;
using Araci.Services.Viewport;

namespace Araci.Services.Editing
{
    public class MoveHudService : INotifyPropertyChanged
    {
        private const double MARGEM = 10;

        private readonly Func<ViewportService?> _viewportProvider;

        private bool _visivel;
        private double _x;
        private double _y;
        private double _deltaX;
        private double _deltaY;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MoveHudService(Func<ViewportService?> viewportProvider)
        {
            _viewportProvider = viewportProvider
                ?? throw new ArgumentNullException(nameof(viewportProvider));
        }

        public bool Visivel
        {
            get => _visivel;
            set
            {
                if (_visivel != value)
                {
                    _visivel = value;
                    OnChanged(nameof(Visivel));
                }
            }
        }

        public double X
        {
            get => _x;
            private set
            {
                if (_x != value)
                {
                    _x = value;
                    OnChanged(nameof(X));
                }
            }
        }

        public double Y
        {
            get => _y;
            private set
            {
                if (_y != value)
                {
                    _y = value;
                    OnChanged(nameof(Y));
                }
            }
        }

        public double DeltaX
        {
            get => _deltaX;
            set
            {
                if (_deltaX != value)
                {
                    _deltaX = value;
                    OnChanged(nameof(DeltaX));
                }
            }
        }

        public double DeltaY
        {
            get => _deltaY;
            set
            {
                if (_deltaY != value)
                {
                    _deltaY = value;
                    OnChanged(nameof(DeltaY));
                }
            }
        }

        // =========================
        // POSICIONAMENTO INTELIGENTE
        // =========================

        public void AtualizarPosicao(Rect bounds)
        {
            ViewportService? viewport = _viewportProvider();

            double viewportX =
                viewport?.Largura ?? 1000;

            double viewportY =
                viewport?.Altura ?? 800;

            Point worldPoint =
                new(
                    bounds.X + bounds.Width / 2,
                    bounds.Y - 20);

            Point screenPoint =
                viewport?.WorldToScreen(worldPoint)
                ?? worldPoint;

            double novoX =
                screenPoint.X;

            double novoY =
                screenPoint.Y;

            // CLAMP VIEWPORT
            novoX = Math.Max(MARGEM, Math.Min(novoX, viewportX - MARGEM));
            novoY = Math.Max(MARGEM, Math.Min(novoY, viewportY - MARGEM));

            X = novoX;
            Y = novoY;
        }

        public void Reset()
        {
            DeltaX = 0;
            DeltaY = 0;
        }

        private void OnChanged(string nome)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nome));
        }
    }
}
