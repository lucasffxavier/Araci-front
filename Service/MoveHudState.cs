using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.Services
{
    public class MoveHudState : INotifyPropertyChanged
    {
        private double _deltaX;
        private double _deltaY;
        private bool _visivel;
        private double _x;
        private double _y;

        public double DeltaX
        {
            get => _deltaX;
            set { _deltaX = value; OnPropertyChanged(); }
        }

        public double DeltaY
        {
            get => _deltaY;
            set { _deltaY = value; OnPropertyChanged(); }
        }

        public bool Visivel
        {
            get => _visivel;
            set { _visivel = value; OnPropertyChanged(); }
        }

        // 🔥 POSIÇÃO DO HUD
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        public void Reset()
        {
            DeltaX = 0;
            DeltaY = 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }
}