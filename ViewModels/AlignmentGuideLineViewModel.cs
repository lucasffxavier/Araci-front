using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.ViewModels
{
    public class AlignmentGuideLineViewModel : INotifyPropertyChanged
    {
        private double _x1;
        private double _y1;
        private double _x2;
        private double _y2;

        public double X1
        {
            get => _x1;
            set
            {
                if (_x1 == value)
                    return;
                _x1 = value;
                OnPropertyChanged();
            }
        }

        public double Y1
        {
            get => _y1;
            set
            {
                if (_y1 == value)
                    return;
                _y1 = value;
                OnPropertyChanged();
            }
        }

        public double X2
        {
            get => _x2;
            set
            {
                if (_x2 == value)
                    return;
                _x2 = value;
                OnPropertyChanged();
            }
        }

        public double Y2
        {
            get => _y2;
            set
            {
                if (_y2 == value)
                    return;
                _y2 = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }
}