using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Models.Tipos;
using Araci.Properties;

namespace Araci.ViewModels
{
    public class TipoLinhaAnotativaViewModel : TipoElementoViewModel
    {
        private readonly Action? _tipoAlterado;
        private readonly string _nomeOriginal;
        private readonly string _familiaOriginal;
        private readonly string _categoriaOriginal;
        private readonly string _estiloLinhaOriginal;
        private readonly string _corLinhaOriginal;
        private readonly double _espessuraLinhaOriginal;
        private SimpleCommand? _escolherCorCommand;

        public TipoLinhaAnotativaViewModel(TipoLinhaAnotativa tipo)
            : this(tipo, null)
        {
        }

        public TipoLinhaAnotativaViewModel(TipoLinhaAnotativa tipo, Action? tipoAlterado)
            : base(tipo)
        {
            _tipoAlterado = tipoAlterado;
            _nomeOriginal = tipo.NomeTipo;
            _familiaOriginal = tipo.Familia;
            _categoriaOriginal = tipo.Categoria;
            _estiloLinhaOriginal = tipo.EstiloLinha;
            _corLinhaOriginal = tipo.CorLinha;
            _espessuraLinhaOriginal = tipo.EspessuraLinha;
        }

        protected TipoLinhaAnotativa TipoLinha => (TipoLinhaAnotativa)_tipo;

        public IReadOnlyList<string> EstilosLinhaDisponiveis { get; } = new[] { "Contínuo", "Tracejado", "Traço ponto", "Traço dois pontos" };

        public ICommand EscolherCorCommand => _escolherCorCommand ??= new SimpleCommand(EscolherCor);

        public string EstiloLinha
        {
            get => TipoLinha.EstiloLinha;
            set
            {
                if (TipoLinha.EstiloLinha == value)
                    return;

                TipoLinha.EstiloLinha = value;
                OnPropertyChanged();
            }
        }

        public string CorLinha
        {
            get => TipoLinha.CorLinha;
            set
            {
                string normalizada = TipoLinhaAnotativa.NormalizarCor(value);

                if (string.Equals(TipoLinha.CorLinha, normalizada, StringComparison.OrdinalIgnoreCase))
                    return;

                TipoLinha.CorLinha = normalizada;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CorLinhaBrush));
            }
        }

        public Brush CorLinhaBrush => CriarBrush(CorLinha);

        public double EspessuraLinha
        {
            get => TipoLinha.EspessuraLinha;
            set
            {
                double normalizada = TipoLinhaAnotativa.NormalizarEspessura(value);

                if (Math.Abs(TipoLinha.EspessuraLinha - normalizada) < 0.000001)
                    return;

                TipoLinha.EspessuraLinha = normalizada;
                OnPropertyChanged();
            }
        }

        public override void CommitChanges()
        {
            _tipoAlterado?.Invoke();
        }

        public override void CancelChanges()
        {
            TipoLinha.NomeTipo = _nomeOriginal;
            TipoLinha.Familia = _familiaOriginal;
            TipoLinha.Categoria = _categoriaOriginal;
            TipoLinha.EstiloLinha = _estiloLinhaOriginal;
            TipoLinha.CorLinha = _corLinhaOriginal;
            TipoLinha.EspessuraLinha = _espessuraLinhaOriginal;
            NotificarTudo();
        }

        private void EscolherCor()
        {
            var window = new ColorPickerWindow(CorLinha)
            {
                Owner = System.Windows.Application.Current?.MainWindow
            };

            if (window.ShowDialog() == true)
                CorLinha = window.SelectedColorHex;
        }

        private void NotificarTudo()
        {
            OnPropertyChanged(nameof(NomeTipo));
            OnPropertyChanged(nameof(Familia));
            OnPropertyChanged(nameof(Categoria));
            OnPropertyChanged(nameof(EstiloLinha));
            OnPropertyChanged(nameof(CorLinha));
            OnPropertyChanged(nameof(CorLinhaBrush));
            OnPropertyChanged(nameof(EspessuraLinha));
        }

        private static Brush CriarBrush(string cor)
        {
            try
            {
                if (ColorConverter.ConvertFromString(cor) is Color color)
                    return new SolidColorBrush(color);
            }
            catch (FormatException)
            {
            }

            return Brushes.Black;
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

            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }
        }
    }
}