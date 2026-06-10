using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.Properties;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateRectanglePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateRectangle _retangulo;
        private readonly MoverRetanguloDoTipoPranchaUseCase _editarRetangulo;
        private SimpleCommand? _escolherCorCommand;

        public ProjectSheetTemplateRectanglePropertiesViewModel(
            ProjectSheetType tipo,
            ProjectSheetTemplateRectangle retangulo,
            MoverRetanguloDoTipoPranchaUseCase editarRetangulo)
        {
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _retangulo = retangulo ?? throw new ArgumentNullException(nameof(retangulo));
            _editarRetangulo = editarRetangulo ?? throw new ArgumentNullException(nameof(editarRetangulo));
        }

        public Guid Id => _retangulo.Id;
        public string Titulo => "Retângulo do Tipo de Prancha";
        public ICommand EscolherCorCommand => _escolherCorCommand ??= new SimpleCommand(EscolherCor);

        public string Nome
        {
            get => NomeAtual();
            set
            {
                if (_editarRetangulo.AlterarNome(_tipo.Id, _retangulo.Id, value))
                    OnPropertyChanged();
            }
        }

        public double X
        {
            get => _retangulo.X;
            set
            {
                if (_editarRetangulo.AlterarPosicao(_tipo.Id, _retangulo.Id, value, _retangulo.Y))
                    OnPositionPropertiesChanged();
            }
        }

        public double Y
        {
            get => _retangulo.Y;
            set
            {
                if (_editarRetangulo.AlterarPosicao(_tipo.Id, _retangulo.Id, _retangulo.X, value))
                    OnPositionPropertiesChanged();
            }
        }

        public double Largura
        {
            get => _retangulo.Largura;
            set
            {
                if (_editarRetangulo.AlterarLargura(_tipo.Id, _retangulo.Id, value))
                    OnPropertyChanged();
            }
        }

        public double Altura
        {
            get => _retangulo.Altura;
            set
            {
                if (_editarRetangulo.AlterarAltura(_tipo.Id, _retangulo.Id, value))
                    OnPropertyChanged();
            }
        }

        public string CorLinha
        {
            get => _retangulo.Stroke;
            set
            {
                if (_editarRetangulo.AlterarStroke(_tipo.Id, _retangulo.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public Brush CorLinhaBrush => CriarBrush(CorLinha);

        public double EspessuraLinha
        {
            get => _retangulo.StrokeThickness;
            set
            {
                if (_editarRetangulo.AlterarEspessura(_tipo.Id, _retangulo.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public bool AindaExiste => _tipo.Retangulos.Any(r => r.Id == _retangulo.Id);

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh()
        {
            OnPropertyChanged(nameof(Nome));
            OnPositionPropertiesChanged();
            OnPropertyChanged(nameof(Largura));
            OnPropertyChanged(nameof(Altura));
            OnStylePropertiesChanged();
            OnPropertyChanged(nameof(AindaExiste));
        }

        private void EscolherCor()
        {
            var window = new ColorPickerWindow(CorLinha)
            {
                Owner = Application.Current?.MainWindow
            };

            if (window.ShowDialog() == true)
                CorLinha = window.SelectedColorHex;
        }

        private string NomeAtual()
        {
            if (!string.IsNullOrWhiteSpace(_retangulo.Nome))
                return _retangulo.Nome;

            int indice = _tipo.Retangulos.FindIndex(r => r.Id == _retangulo.Id);

            return indice >= 0
                ? $"RETANGULO-{indice + 1:000}"
                : "RETANGULO";
        }

        private void OnPositionPropertiesChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        private void OnStylePropertiesChanged()
        {
            OnPropertyChanged(nameof(CorLinha));
            OnPropertyChanged(nameof(CorLinhaBrush));
            OnPropertyChanged(nameof(EspessuraLinha));
        }

        private static Brush CriarBrush(string stroke)
        {
            try
            {
                if (ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(stroke) ? "#FF000000" : stroke) is Color color)
                    return new SolidColorBrush(color);
            }
            catch (FormatException)
            {
            }

            return Brushes.Black;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class SimpleCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool>? _canExecute;

            public SimpleCommand(Action execute, Func<bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object? parameter)
            {
                return _canExecute?.Invoke() ?? true;
            }

            public void Execute(object? parameter)
            {
                if (CanExecute(parameter))
                    _execute();
            }

            public event EventHandler? CanExecuteChanged;
        }
    }
}