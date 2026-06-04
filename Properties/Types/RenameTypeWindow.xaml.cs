using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Araci.Properties.Types
{
    public partial class RenameTypeWindow : Window, INotifyPropertyChanged
    {
        private string _novoNome;
        private string _mensagemErro = string.Empty;

        public RenameTypeWindow(string nomeAtual)
        {
            NomeAtual = nomeAtual ?? string.Empty;
            _novoNome = NomeAtual;
            InitializeComponent();
            DataContext = this;
            Loaded += OnLoaded;
        }

        public string NomeAtual { get; }

        public string NovoNome
        {
            get => _novoNome;
            set
            {
                if (_novoNome == value)
                    return;

                _novoNome = value;
                OnPropertyChanged();
                MensagemErro = string.Empty;
            }
        }

        public string MensagemErro
        {
            get => _mensagemErro;
            set
            {
                if (_mensagemErro == value)
                    return;

                _mensagemErro = value;
                OnPropertyChanged();
            }
        }

        public void DefinirErro(string mensagem)
        {
            MensagemErro = mensagem;
            NovoNomeTextBox.Focus();
            NovoNomeTextBox.SelectAll();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NovoNomeTextBox.Focus();
            NovoNomeTextBox.SelectAll();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelarClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
