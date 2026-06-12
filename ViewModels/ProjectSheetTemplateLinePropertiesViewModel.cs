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
    public sealed class ProjectSheetTemplateLinePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateLine _linha;
        private readonly MoverLinhaDoTipoPranchaUseCase _editarLinha;
        private readonly TypeLibraryService _types;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private SimpleCommand? _abrirPropriedadesTipoCommand;
        private SimpleCommand? _escolherCorCommand;

        public ProjectSheetTemplateLinePropertiesViewModel(
            ProjectSheetType tipo,
            ProjectSheetTemplateLine linha,
            MoverLinhaDoTipoPranchaUseCase editarLinha,
            TypeLibraryService types,
            TypePropertiesDialogService typePropertiesDialogs)
        {
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
            _editarLinha = editarLinha ?? throw new ArgumentNullException(nameof(editarLinha));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));

            foreach (TipoLinhaAnotativa tipoLinha in _types.TiposLinhasAnotativas)
                tipoLinha.PropertyChanged += OnTipoLinhaPropertyChanged;
        }

        public Guid Id => _linha.Id;
        public string Titulo => "Linha do Tipo de Prancha";
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

                if (_editarLinha.AlterarTipoGrafico(_tipo.Id, _linha.Id, value))
                    Refresh();
            }
        }

        public string Nome
        {
            get => NomeAtual();
            set
            {
                if (_editarLinha.AlterarNome(_tipo.Id, _linha.Id, value))
                    OnPropertyChanged();
            }
        }

        public double Comprimento
        {
            get
            {
                double dx = _linha.X2 - _linha.X1;
                double dy = _linha.Y2 - _linha.Y1;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        public string ComprimentoTexto => UnitFormatter.FormatSheetMillimeters(Comprimento);

        public string CorLinha
        {
            get => _linha.Stroke;
            set
            {
                if (_editarLinha.AlterarStroke(_tipo.Id, _linha.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public Brush CorLinhaBrush => CriarBrush(CorLinha);

        public double EspessuraLinha
        {
            get => _linha.StrokeThickness;
            set
            {
                if (_editarLinha.AlterarEspessura(_tipo.Id, _linha.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public bool AindaExiste => _tipo.Linhas.Any(l => l.Id == _linha.Id);

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh()
        {
            OnPropertyChanged(nameof(TipoLinha));
            OnPropertyChanged(nameof(PodeAbrirPropriedadesTipo));
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(Comprimento));
            OnPropertyChanged(nameof(ComprimentoTexto));
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
            _editarLinha.AlterarTipoGrafico(_tipo.Id, _linha.Id, tipoLinha);
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
            if (_linha.PossuiTipoLinha)
            {
                TipoLinhaAnotativa? tipo = _types.TiposLinhasAnotativas.FirstOrDefault(t =>
                    string.Equals(t.NomeTipo, _linha.TipoLinhaNome, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Familia, _linha.TipoLinhaFamilia, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Categoria, _linha.TipoLinhaCategoria, StringComparison.OrdinalIgnoreCase));

                if (tipo != null)
                    return tipo;
            }

            return _types.TipoLinhaAnotativaPadrao;
        }

        private string NomeAtual()
        {
            if (!string.IsNullOrWhiteSpace(_linha.Nome))
                return _linha.Nome;

            int indice = _tipo.Linhas.FindIndex(l => l.Id == _linha.Id);

            return indice >= 0
                ? $"LINHA-{indice + 1:000}"
                : "LINHA";
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