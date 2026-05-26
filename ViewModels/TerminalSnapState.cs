using System.ComponentModel;
using System.Runtime.CompilerServices;
using Araci.Models;

namespace Araci.ViewModels
{
    public class TerminalSnapState : INotifyPropertyChanged
    {
        private bool _visivel;
        private double _x;
        private double _y;
        private string? _terminalId;
        private string? _elementoId;

        public bool Visivel
        {
            get => _visivel;
            private set
            {
                if (_visivel == value)
                    return;

                _visivel = value;
                OnPropertyChanged();
            }
        }

        public double X
        {
            get => _x;
            private set
            {
                if (System.Math.Abs(_x - value) < 0.0001)
                    return;

                _x = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _y;
            private set
            {
                if (System.Math.Abs(_y - value) < 0.0001)
                    return;

                _y = value;
                OnPropertyChanged();
            }
        }

        public string? TerminalId
        {
            get => _terminalId;
            private set
            {
                if (_terminalId == value)
                    return;

                _terminalId = value;
                OnPropertyChanged();
            }
        }

        public string? ElementoId
        {
            get => _elementoId;
            private set
            {
                if (_elementoId == value)
                    return;

                _elementoId = value;
                OnPropertyChanged();
            }
        }

        public void Mostrar(Terminal terminal)
        {
            X = terminal.Posicao.X;
            Y = terminal.Posicao.Y;
            TerminalId = terminal.Id;
            ElementoId = terminal.Dono.Id.ToString();
            Visivel = true;
        }

        public void Limpar()
        {
            Visivel = false;
            TerminalId = null;
            ElementoId = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
