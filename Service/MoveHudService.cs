using System.ComponentModel;
using System.Windows;

namespace Araci.Services
{
    public class MoveHudService : INotifyPropertyChanged
    {
        private const double MARGEM = 10;

        private bool _visivel;
        private double _x;
        private double _y;
        private double _deltaX;
        private double _deltaY;

        public event PropertyChangedEventHandler? PropertyChanged;

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
            double viewportX =
                AppServices.Viewport?.Largura ?? 1000;

            double viewportY =
                AppServices.Viewport?.Altura ?? 800;

            double novoX =
                bounds.X + bounds.Width / 2;

            double novoY =
                bounds.Y - 20; // acima do elemento

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