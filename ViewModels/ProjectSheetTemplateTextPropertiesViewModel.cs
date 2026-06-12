using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.Models.Tipos;
using Araci.Services.Catalog;
using Araci.Services.Settings;
using Araci.Services.UI;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateTextPropertiesViewModel : INotifyPropertyChanged
    {
        private static readonly IReadOnlyList<BooleanPropertyOption> BooleanOptionsPadrao = new[]
        {
            new BooleanPropertyOption(true, "Sim"),
            new BooleanPropertyOption(false, "Não")
        };

        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateText _texto;
        private readonly MoverTextoDoTipoPranchaUseCase _editarTexto;
        private readonly TypeLibraryService _types;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private SimpleCommand? _abrirPropriedadesTipoCommand;

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
        public string Titulo => "1 Texto selecionado";
        public IReadOnlyList<TipoTextoAnotativo> TiposDisponiveis => _types.TiposTextosAnotativos;
        public IReadOnlyList<BooleanPropertyOption> BooleanOptions => BooleanOptionsPadrao;
        public ICommand AbrirPropriedadesTipoCommand => _abrirPropriedadesTipoCommand ??= new SimpleCommand(AbrirPropriedadesTipo, () => PodeAbrirPropriedadesTipo);
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
                    OnLarguraCaixaChanged();
            }
        }

        public string LarguraCaixaTexto
        {
            get => FormatarComprimentoFolha(LarguraCaixa);
            set
            {
                if (TentarConverterComprimentoFolha(value, out double largura))
                    LarguraCaixa = largura;
            }
        }

        public double Rotacao
        {
            get => NormalizarRotacao(_texto.Rotacao);
            set
            {
                if (_editarTexto.AlterarRotacao(_tipo.Id, _texto.Id, value))
                    OnRotacaoChanged();
            }
        }

        public string RotacaoTexto
        {
            get => Rotacao.ToString("0.00", CultureInfo.CurrentCulture);
            set
            {
                if (PropertiesViewModel.TentarConverterValor(value, typeof(double), out object? convertido) && convertido is double rotacao)
                    Rotacao = rotacao;
            }
        }

        public bool LeaderAtivo
        {
            get => _texto.LeaderAtivo;
            set
            {
                if (_editarTexto.AlterarLeaderAtivo(_tipo.Id, _texto.Id, value))
                    OnPropertyChanged();
            }
        }

        public bool LeaderComCotovelo
        {
            get => _texto.LeaderComCotovelo;
            set
            {
                if (_editarTexto.AlterarLeaderComCotovelo(_tipo.Id, _texto.Id, value))
                    OnPropertyChanged();
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
            OnLarguraCaixaChanged();
            OnRotacaoChanged();
            OnPropertyChanged(nameof(LeaderAtivo));
            OnPropertyChanged(nameof(LeaderComCotovelo));
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
            Refresh();
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

        private void OnLarguraCaixaChanged()
        {
            OnPropertyChanged(nameof(LarguraCaixa));
            OnPropertyChanged(nameof(LarguraCaixaTexto));
        }

        private void OnRotacaoChanged()
        {
            OnPropertyChanged(nameof(Rotacao));
            OnPropertyChanged(nameof(RotacaoTexto));
        }

        private static string FormatarComprimentoFolha(double valor)
        {
            return UnitFormatter.FormatSheetMillimeters(valor);
        }

        private static bool TentarConverterComprimentoFolha(string valor, out double convertido)
        {
            if (UnitFormatter.TryParseSheetMillimeters(valor, out double valorConvertido))
            {
                convertido = valorConvertido;
                return true;
            }

            convertido = 0.0;
            return false;
        }

        private static double NormalizarRotacao(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
                return 0.0;

            double normalizada = valor % 360.0;

            if (normalizada < 0.0)
                normalizada += 360.0;

            return normalizada >= 360.0 ? 0.0 : normalizada;
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