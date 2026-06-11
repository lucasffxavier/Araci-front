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
using Araci.Services.UI;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateTextPropertiesViewModel : INotifyPropertyChanged
    {
        private static readonly string[] FontesPadrao =
        {
            "Arial",
            "Arial Narrow",
            "Calibri",
            "Segoe UI",
            "Courier New",
            "Times New Roman",
            "ISOCP",
            "ISOCPEUR",
            "Romans",
            "Simplex"
        };

        private static readonly string[] AlinhamentosPadrao =
        {
            "Esquerda",
            "Centro",
            "Direita"
        };

        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateText _texto;
        private readonly MoverTextoDoTipoPranchaUseCase _editarTexto;
        private readonly TypeLibraryService _types;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private SimpleCommand? _abrirPropriedadesTipoCommand;
        private SimpleCommand? _escolherCorCommand;

        public ProjectSheetTemplateTextPropertiesViewModel(
            ProjectSheetType tipo,
            ProjectSheetTemplateText texto,
            MoverTextoDoTipoPranchaUseCase editarTexto,
            TypeLibraryService types,
            TypePropertiesDialogService typePropertiesDialogs)
        {
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _texto = texto ?? throw new ArgumentNullException(nameof(texto));
            _editarTexto = editarTexto ?? throw new ArgumentNullException(nameof(editarTexto));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));
        }

        public Guid Id => _texto.Id;
        public string Titulo => "Texto do Tipo de Prancha";
        public IReadOnlyList<TipoTextoAnotativo> TiposDisponiveis => _types.TiposTextosAnotativos;
        public IReadOnlyList<string> FontesDisponiveis => FontesPadrao;
        public IReadOnlyList<string> AlinhamentosDisponiveis => AlinhamentosPadrao;
        public ICommand AbrirPropriedadesTipoCommand => _abrirPropriedadesTipoCommand ??= new SimpleCommand(AbrirPropriedadesTipo, () => PodeAbrirPropriedadesTipo);
        public ICommand EscolherCorCommand => _escolherCorCommand ??= new SimpleCommand(EscolherCor);
        public bool PodeAbrirPropriedadesTipo => TipoTexto != null;

        public TipoTextoAnotativo? TipoTexto
        {
            get => ResolverTipoTexto();
            set
            {
                if (value == null)
                    return;

                if (_editarTexto.AlterarTipoTexto(_tipo.Id, _texto.Id, value))
                    Refresh();
            }
        }

        public string Nome
        {
            get => NomeAtual();
            set
            {
                if (_editarTexto.AlterarNome(_tipo.Id, _texto.Id, value))
                    OnPropertyChanged();
            }
        }

        public string Conteudo
        {
            get => _texto.Texto;
            set
            {
                if (_editarTexto.AlterarConteudo(_tipo.Id, _texto.Id, value))
                    OnPropertyChanged();
            }
        }

        public double LarguraCaixa
        {
            get => _texto.LarguraCaixa;
            set
            {
                if (_editarTexto.AlterarLarguraCaixa(_tipo.Id, _texto.Id, value))
                    OnPropertyChanged();
            }
        }

        public string CorTexto
        {
            get => _texto.CorTexto;
            set
            {
                if (_editarTexto.AlterarCorTexto(_tipo.Id, _texto.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public Brush CorTextoBrush => CriarBrush(CorTexto);

        public string Fonte
        {
            get => _texto.Fonte;
            set
            {
                if (_editarTexto.AlterarFonte(_tipo.Id, _texto.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public double AlturaTexto
        {
            get => _texto.AlturaTexto;
            set
            {
                if (_editarTexto.AlterarAlturaTexto(_tipo.Id, _texto.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public string AlinhamentoHorizontal
        {
            get => _texto.AlinhamentoHorizontal;
            set
            {
                if (_editarTexto.AlterarAlinhamentoHorizontal(_tipo.Id, _texto.Id, value))
                    OnStylePropertiesChanged();
            }
        }

        public bool AindaExiste => _tipo.Textos.Any(t => t.Id == _texto.Id);

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh()
        {
            OnPropertyChanged(nameof(TipoTexto));
            OnPropertyChanged(nameof(PodeAbrirPropriedadesTipo));
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(Conteudo));
            OnPropertyChanged(nameof(LarguraCaixa));
            OnStylePropertiesChanged();
            OnPropertyChanged(nameof(AindaExiste));
            _abrirPropriedadesTipoCommand?.RaiseCanExecuteChanged();
        }

        private void AbrirPropriedadesTipo()
        {
            TipoTextoAnotativo? tipoTexto = TipoTexto;

            if (tipoTexto == null)
                return;

            _typePropertiesDialogs.Show(new TipoTextoAnotativoViewModel(
                tipoTexto,
                _types.TiposTextosAnotativos,
                SelecionarTipoTextoDoDialogo,
                Refresh));

            _editarTexto.AlterarTipoTexto(_tipo.Id, _texto.Id, tipoTexto);
            Refresh();
        }

        private void SelecionarTipoTextoDoDialogo(TipoTextoAnotativo tipoTexto)
        {
            if (tipoTexto == null)
                return;

            _editarTexto.AlterarTipoTexto(_tipo.Id, _texto.Id, tipoTexto);
        }

        private void EscolherCor()
        {
            var window = new ColorPickerWindow(CorTexto)
            {
                Owner = Application.Current?.MainWindow
            };

            if (window.ShowDialog() == true)
                CorTexto = window.SelectedColorHex;
        }

        private TipoTextoAnotativo? ResolverTipoTexto()
        {
            if (_texto.PossuiTipoTexto)
            {
                TipoTextoAnotativo? tipo = _types.TiposTextosAnotativos.FirstOrDefault(t =>
                    string.Equals(t.NomeTipo, _texto.TipoTextoNome, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Familia, _texto.TipoTextoFamilia, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Categoria, _texto.TipoTextoCategoria, StringComparison.OrdinalIgnoreCase));

                if (tipo != null)
                    return tipo;
            }

            return _types.TipoTextoAnotativoPadrao;
        }

        private string NomeAtual()
        {
            if (!string.IsNullOrWhiteSpace(_texto.Nome))
                return _texto.Nome;

            int indice = _tipo.Textos.FindIndex(t => t.Id == _texto.Id);

            return indice >= 0
                ? $"TEXTO-{indice + 1:000}"
                : "TEXTO";
        }

        private void OnStylePropertiesChanged()
        {
            OnPropertyChanged(nameof(CorTexto));
            OnPropertyChanged(nameof(CorTextoBrush));
            OnPropertyChanged(nameof(Fonte));
            OnPropertyChanged(nameof(AlturaTexto));
            OnPropertyChanged(nameof(AlinhamentoHorizontal));
        }

        private static Brush CriarBrush(string stroke)
        {
            try
            {
                if (ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(stroke) ? ProjectSheetTemplateText.DefaultTextColor : stroke) is Color color)
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