using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Models.Tipos;
using Araci.Properties;

namespace Araci.ViewModels
{
    public class TipoTextoAnotativoViewModel : TipoElementoViewModel
    {
        private readonly ObservableCollection<TipoTextoAnotativo>? _tiposDisponiveis;
        private readonly Action<TipoTextoAnotativo>? _selecionarTipo;
        private ICommand? _escolherCorCommand;
        private ICommand? _novoTipoCommand;

        public TipoTextoAnotativoViewModel(TipoTextoAnotativo tipo)
            : this(tipo, null, null)
        {
        }

        public TipoTextoAnotativoViewModel(TipoTextoAnotativo tipo, ObservableCollection<TipoTextoAnotativo>? tiposDisponiveis, Action<TipoTextoAnotativo>? selecionarTipo)
            : base(tipo)
        {
            _tiposDisponiveis = tiposDisponiveis;
            _selecionarTipo = selecionarTipo;
        }

        protected TipoTextoAnotativo TipoTexto => (TipoTextoAnotativo)_tipo;

        public IEnumerable<TipoTextoAnotativo> TiposDisponiveis => _tiposDisponiveis != null ? _tiposDisponiveis : new[] { TipoTexto };

        public TipoTextoAnotativo TipoSelecionado
        {
            get => TipoTexto;
            set
            {
                if (value == null || ReferenceEquals(TipoTexto, value))
                    return;

                AtualizarTipoBase(value);
                _selecionarTipo?.Invoke(value);
                NotificarTudo();
            }
        }

        public IReadOnlyList<string> FontesDisponiveis { get; } = new[]
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

        public IReadOnlyList<string> AlinhamentosDisponiveis { get; } = new[]
        {
            "Esquerda",
            "Centro",
            "Direita"
        };

        public ICommand EscolherCorCommand => _escolherCorCommand ??= new SimpleCommand(EscolherCor);
        public ICommand NovoTipoCommand => _novoTipoCommand ??= new SimpleCommand(CriarNovoTipo, () => _tiposDisponiveis != null);

        public string CorTexto
        {
            get => TipoTexto.CorTexto;
            set
            {
                if (!ColorPickerWindow.TryNormalizeHexColor(value, out string normalizada))
                    normalizada = "#FF000000";

                if (TipoTexto.CorTexto == normalizada)
                    return;

                TipoTexto.CorTexto = normalizada;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CorTextoBrush));
            }
        }

        public Brush CorTextoBrush => CriarBrush(CorTexto);

        public string Fonte
        {
            get => TipoTexto.Fonte;
            set
            {
                if (TipoTexto.Fonte == value)
                    return;

                TipoTexto.Fonte = value;
                OnPropertyChanged();
            }
        }

        public double AlturaTexto
        {
            get => TipoTexto.AlturaTexto;
            set
            {
                if (Math.Abs(TipoTexto.AlturaTexto - value) < 0.0001)
                    return;

                TipoTexto.AlturaTexto = value;
                OnPropertyChanged();
            }
        }

        public string AlinhamentoHorizontal
        {
            get => TipoTexto.AlinhamentoHorizontal;
            set
            {
                if (TipoTexto.AlinhamentoHorizontal == value)
                    return;

                TipoTexto.AlinhamentoHorizontal = value;
                OnPropertyChanged();
            }
        }

        private void CriarNovoTipo()
        {
            if (_tiposDisponiveis == null)
                return;

            var novo = new TipoTextoAnotativo
            {
                NomeTipo = GerarNomeUnico(TipoTexto.NomeTipo),
                Familia = TipoTexto.Familia,
                Categoria = TipoTexto.Categoria,
                CorTexto = TipoTexto.CorTexto,
                Fonte = TipoTexto.Fonte,
                AlturaTexto = TipoTexto.AlturaTexto,
                AlinhamentoHorizontal = TipoTexto.AlinhamentoHorizontal
            };

            _tiposDisponiveis.Add(novo);
            OnPropertyChanged(nameof(TiposDisponiveis));
            TipoSelecionado = novo;
        }

        private string GerarNomeUnico(string nomeBase)
        {
            string baseLimpa = string.IsNullOrWhiteSpace(nomeBase) ? "Texto" : nomeBase.Trim();
            var existentes = (_tiposDisponiveis ?? new ObservableCollection<TipoTextoAnotativo>())
                .Select(t => t.NomeTipo)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            int indice = 2;
            string candidato;

            do
            {
                candidato = $"{baseLimpa} {indice}";
                indice++;
            }
            while (existentes.Contains(candidato));

            return candidato;
        }

        private void EscolherCor()
        {
            var window = new ColorPickerWindow(CorTexto)
            {
                Owner = System.Windows.Application.Current?.MainWindow
            };

            if (window.ShowDialog() == true)
                CorTexto = window.SelectedColorHex;
        }

        private void NotificarTudo()
        {
            OnPropertyChanged(nameof(TipoSelecionado));
            OnPropertyChanged(nameof(TiposDisponiveis));
            OnPropertyChanged(nameof(NomeTipo));
            OnPropertyChanged(nameof(Familia));
            OnPropertyChanged(nameof(Categoria));
            OnPropertyChanged(nameof(CorTexto));
            OnPropertyChanged(nameof(CorTextoBrush));
            OnPropertyChanged(nameof(Fonte));
            OnPropertyChanged(nameof(AlturaTexto));
            OnPropertyChanged(nameof(AlinhamentoHorizontal));
        }

        private static Brush CriarBrush(string cor)
        {
            try
            {
                object? valor = ColorConverter.ConvertFromString(cor);

                if (valor is Color color)
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