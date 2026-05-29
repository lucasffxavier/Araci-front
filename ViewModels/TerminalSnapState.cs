using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Araci.Models;

namespace Araci.ViewModels
{
    public class TerminalSnapState : INotifyPropertyChanged
    {
        private bool _visivel;
        private bool _invalido;
        private bool _mensagemVisivel;
        private double _x;
        private double _y;
        private double _mensagemX;
        private double _mensagemY;
        private string? _terminalId;
        private string? _elementoId;
        private string _mensagem = string.Empty;

        public bool Visivel
        {
            get => _visivel;
            private set
            {
                if (_visivel == value)
                    return;

                _visivel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MarcadorValidoVisivel));
                OnPropertyChanged(nameof(MarcadorInvalidoVisivel));
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

        public bool Invalido
        {
            get => _invalido;
            private set
            {
                if (_invalido == value)
                    return;

                _invalido = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MarcadorValidoVisivel));
                OnPropertyChanged(nameof(MarcadorInvalidoVisivel));
            }
        }

        public bool MarcadorValidoVisivel => Visivel && !Invalido;
        public bool MarcadorInvalidoVisivel => Visivel && Invalido;

        public bool MensagemVisivel
        {
            get => _mensagemVisivel;
            private set
            {
                if (_mensagemVisivel == value)
                    return;

                _mensagemVisivel = value;
                OnPropertyChanged();
            }
        }

        public string Mensagem
        {
            get => _mensagem;
            private set
            {
                if (_mensagem == value)
                    return;

                _mensagem = value;
                OnPropertyChanged();
            }
        }

        public double MensagemX
        {
            get => _mensagemX;
            private set
            {
                if (System.Math.Abs(_mensagemX - value) < 0.0001)
                    return;

                _mensagemX = value;
                OnPropertyChanged();
            }
        }

        public double MensagemY
        {
            get => _mensagemY;
            private set
            {
                if (System.Math.Abs(_mensagemY - value) < 0.0001)
                    return;

                _mensagemY = value;
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
            MensagemX = terminal.Posicao.X;
            MensagemY = terminal.Posicao.Y;
            TerminalId = terminal.Id;
            ElementoId = terminal.Dono.Id.ToString();
            Invalido = false;
            MensagemVisivel = false;
            Mensagem = string.Empty;
            Visivel = true;
        }

        public void MostrarInvalido(Terminal terminal, Point cursor, string mensagem)
        {
            X = terminal.Posicao.X;
            Y = terminal.Posicao.Y;
            MensagemX = terminal.Posicao.X;
            MensagemY = terminal.Posicao.Y;
            TerminalId = terminal.Id;
            ElementoId = terminal.Dono.Id.ToString();
            Invalido = true;
            Mensagem = mensagem;
            MensagemVisivel = true;
            Visivel = true;
        }

        public void Limpar()
        {
            Visivel = false;
            Invalido = false;
            MensagemVisivel = false;
            Mensagem = string.Empty;
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