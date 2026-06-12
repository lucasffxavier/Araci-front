using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.Models.Tipos;
using Araci.Properties;
using Araci.Services.Catalog;
using Araci.Services.Settings;
using Araci.Services.UI;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateRectanglePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateRectangle _retangulo;
        private readonly MoverRetanguloDoTipoPranchaUseCase _editarRetangulo;
        private readonly TypeLibraryService _types;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private SimpleCommand? _abrirPropriedadesTipoCommand;
        private SimpleCommand? _escolherCorCommand;

        public ProjectSheetTemplateRectanglePropertiesViewModel(
            ProjectSheetType tipo,
            ProjectSheetTemplateRectangle retangulo,
            MoverRetanguloDoTipoPranchaUseCase editarRetangulo,
            TypeLibraryService types,
            TypePropertiesDialogService typePropertiesDialogs)
        {
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _retangulo = retangulo ?? throw new ArgumentNullException(nameof(retangulo));
            _editarRetangulo = editarRetangulo ?? throw new ArgumentNullException(nameof(editarRetangulo));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));

            foreach (TipoLinhaAnotativa tipoLinha in _types.TiposLinhasAnotativas)
                tipoLinha.PropertyChanged += OnTipoLinhaPropertyChanged;
        }

        public Guid Id => _retangulo.Id;
        public string Titulo => "Retângulo do Tipo de Prancha";
        public IReadOnlyList<TipoLinhaAnotativa> TiposDisponiveis => _types.TiposLinhasAnotativas;
        public ICommand AbrirPropriedadesTipoCommand => _abrirPropriedadesTipoCommand ??= new SimpleCommand(AbrirPropriedadesTipo, () => PodeAbrirPropriedadesTipo);
        public ICommand EscolherCorCommand => _escolherCorCommand ??= new SimpleCommand(EscolherCor);

        public bool PodeAbrirPropriedadesTipo => TipoLinha != null;

        public TipoLinhaAnotativa? TipoLinha
        {
            get => ResolverTipoLinha();
            set
            {
                if (value == null)
                    return;

                if (_editarRetangulo.AlterarTipoGrafico(_tipo.Id, _retangulo.Id, value))
                    Refresh();
            }
        }

        public string Nome
        {
            get => NomeAtual();
            set
            {
                if (_editarRetangulo.AlterarNome(_tipo.Id, _retangulo.Id, value))
                    OnPropertyChanged();
            }
        }

        public double Largura
        {
            get => _retangulo.Largura;
            set
            {
                if (_editarRetangulo.AlterarLargura(_tipo.Id, _retangulo.Id, value))
                    OnGeometryPropertiesChanged();
            }
        }

        public double Altura
        {
            get => _retangulo.Altura;
            set
            {
                if (_editarRetangulo.AlterarAltura(_tipo.Id, _retangulo.Id, value))
                    OnGeometryPropertiesChanged();
            }
        }

        public string LarguraTexto
        {
            get => UnitFormatter.FormatSheetMillimeters(_retangulo.Largura);
            set
            {
                if (!UnitFormatter.TryParseSheetMillimeters(value, out double largura))
                {
                    OnPropertyChanged();
                    return;
                }

                if (_editarRetangulo.AlterarLargura(_tipo.Id, _retangulo.Id, largura))
                    OnGeometryPropertiesChanged();
                else
                    OnPropertyChanged();
            }
        }

        public string AlturaTexto
        {
            get => UnitFormatter.FormatSheetMillimeters(_retangulo.Altura);
            set
            {
                if (!UnitFormatter.TryParseSheetMillimeters(value, out double altura))
                {
                    OnPropertyChanged();
                    return;
                }

                if (_editarRetangulo.AlterarAltura(_tipo.Id, _retangulo.Id, altura))
                    OnGeometryPropertiesChanged();
                else
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
            OnPropertyChanged(nameof(TipoLinha));
            OnPropertyChanged(nameof(PodeAbrirPropriedadesTipo));
            OnPropertyChanged(nameof(Nome));
            OnGeometryPropertiesChanged();
            OnStylePropertiesChanged();
            OnPropertyChanged(nameof(AindaExiste));
            _abrirPropriedadesTipoCommand?.RaiseCanExecuteChanged();
        }

        private void AbrirPropriedadesTipo()
        {
            TipoLinhaAnotativa? tipoLinha = TipoLinha;

            if (tipoLinha == null)
                return;

            _typePropertiesDialogs.Show(new TipoLinhaAnotativaViewModel(tipoLinha, Refresh));
            _editarRetangulo.AlterarTipoGrafico(_tipo.Id, _retangulo.Id, tipoLinha);
            Refresh();
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

        private TipoLinhaAnotativa? ResolverTipoLinha()
        {
            if (_retangulo.PossuiTipoLinha)
            {
                TipoLinhaAnotativa? tipo = _types.TiposLinhasAnotativas.FirstOrDefault(t =>
                    string.Equals(t.NomeTipo, _retangulo.TipoLinhaNome, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Familia, _retangulo.TipoLinhaFamilia, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Categoria, _retangulo.TipoLinhaCategoria, StringComparison.OrdinalIgnoreCase));

                if (tipo != null)
                    return tipo;
            }

            return _types.TipoLinhaAnotativaPadrao;
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

        private void OnTipoLinhaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            TipoLinhaAnotativa? tipoLinha = TipoLinha;

            if (tipoLinha == null || !ReferenceEquals(sender, tipoLinha))
                return;

            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.EstiloLinha) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.NomeTipo))
            {
                Refresh();
            }
        }

        private void OnGeometryPropertiesChanged()
        {
            OnPropertyChanged(nameof(Largura));
            OnPropertyChanged(nameof(Altura));
            OnPropertyChanged(nameof(LarguraTexto));
            OnPropertyChanged(nameof(AlturaTexto));
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

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler? CanExecuteChanged;
        }
    }
}