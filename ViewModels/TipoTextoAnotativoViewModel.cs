using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Models.Tipos;
using Araci.Properties;
using Araci.Properties.Types;

namespace Araci.ViewModels
{
    public class TipoTextoAnotativoViewModel : TipoElementoViewModel
    {
        private readonly ObservableCollection<TipoTextoAnotativo>? _tiposReais;
        private readonly ObservableCollection<TipoTextoAnotativo>? _tiposTemporarios;
        private readonly Dictionary<TipoTextoAnotativo, TipoTextoAnotativo?> _mapaTemporarioParaReal = new();
        private readonly Action<TipoTextoAnotativo>? _selecionarTipo;
        private readonly Action? _tipoAlterado;
        private ICommand? _escolherCorCommand;
        private ICommand? _novoTipoCommand;
        private ICommand? _renomearTipoCommand;

        public TipoTextoAnotativoViewModel(TipoTextoAnotativo tipo)
            : this(tipo, null, null, null)
        {
        }

        public TipoTextoAnotativoViewModel(TipoTextoAnotativo tipo, ObservableCollection<TipoTextoAnotativo>? tiposDisponiveis, Action<TipoTextoAnotativo>? selecionarTipo)
            : this(tipo, tiposDisponiveis, selecionarTipo, null)
        {
        }

        public TipoTextoAnotativoViewModel(TipoTextoAnotativo tipo, ObservableCollection<TipoTextoAnotativo>? tiposDisponiveis, Action<TipoTextoAnotativo>? selecionarTipo, Action? tipoAlterado)
            : base(tipo)
        {
            _tiposReais = tiposDisponiveis;
            _selecionarTipo = selecionarTipo;
            _tipoAlterado = tipoAlterado;

            if (_tiposReais != null)
            {
                _tiposTemporarios = new ObservableCollection<TipoTextoAnotativo>();

                foreach (TipoTextoAnotativo tipoReal in _tiposReais)
                {
                    TipoTextoAnotativo temporario = ClonarTipo(tipoReal);
                    _tiposTemporarios.Add(temporario);
                    _mapaTemporarioParaReal[temporario] = tipoReal;

                    if (ReferenceEquals(tipoReal, tipo))
                        _tipo = temporario;
                }

                if (!_mapaTemporarioParaReal.ContainsKey((TipoTextoAnotativo)_tipo))
                {
                    TipoTextoAnotativo temporarioSelecionado = ClonarTipo(tipo);
                    _tiposTemporarios.Add(temporarioSelecionado);
                    _mapaTemporarioParaReal[temporarioSelecionado] = tipo;
                    _tipo = temporarioSelecionado;
                }
            }
        }

        protected TipoTextoAnotativo TipoTexto => (TipoTextoAnotativo)_tipo;

        public IEnumerable<TipoTextoAnotativo> TiposDisponiveis => _tiposTemporarios != null ? _tiposTemporarios : new[] { TipoTexto };

        public TipoTextoAnotativo TipoSelecionado
        {
            get => TipoTexto;
            set
            {
                if (value == null || ReferenceEquals(TipoTexto, value))
                    return;

                AtualizarTipoBase(value);
                NotificarTudo();
            }
        }

        public string TipoSelecionadoNome => TipoTexto.NomeTipo;

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
        public ICommand NovoTipoCommand => _novoTipoCommand ??= new SimpleCommand(CriarNovoTipo, () => _tiposTemporarios != null);
        public ICommand RenomearTipoCommand => _renomearTipoCommand ??= new SimpleCommand(RenomearTipo, () => _tiposTemporarios != null);

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

        public override void CommitChanges()
        {
            if (_tiposTemporarios == null || _tiposReais == null)
            {
                _tipoAlterado?.Invoke();
                return;
            }

            TipoTextoAnotativo selecionadoTemporario = TipoTexto;
            TipoTextoAnotativo? selecionadoReal = null;

            foreach (TipoTextoAnotativo temporario in _tiposTemporarios)
            {
                if (!_mapaTemporarioParaReal.TryGetValue(temporario, out TipoTextoAnotativo? real) || real == null)
                {
                    real = ClonarTipo(temporario);
                    _tiposReais.Add(real);
                    _mapaTemporarioParaReal[temporario] = real;
                }
                else
                {
                    CopiarValores(temporario, real);
                }

                if (ReferenceEquals(temporario, selecionadoTemporario))
                    selecionadoReal = real;
            }

            CollectionViewSource.GetDefaultView(_tiposReais)?.Refresh();

            if (selecionadoReal != null)
                _selecionarTipo?.Invoke(selecionadoReal);

            _tipoAlterado?.Invoke();
        }

        public override void CancelChanges()
        {
        }

        private void CriarNovoTipo()
        {
            if (_tiposTemporarios == null)
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

            _tiposTemporarios.Add(novo);
            _mapaTemporarioParaReal[novo] = null;
            AtualizarListaDeTipos();
            TipoSelecionado = novo;
        }

        private void RenomearTipo()
        {
            if (_tiposTemporarios == null)
                return;

            var window = new RenameTypeWindow(TipoTexto.NomeTipo)
            {
                Owner = Application.Current?.MainWindow
            };

            while (window.ShowDialog() == true)
            {
                string novoNome = NormalizarNome(window.NovoNome);

                if (string.IsNullOrWhiteSpace(novoNome))
                {
                    window.DefinirErro("Informe um nome para o tipo.");
                    continue;
                }

                if (ExisteNomeDuplicado(novoNome))
                {
                    window.DefinirErro("Já existe um tipo com esse nome.");
                    continue;
                }

                NomeTipo = novoNome;
                AtualizarListaDeTipos();
                OnPropertyChanged(nameof(TipoSelecionado));
                OnPropertyChanged(nameof(TipoSelecionadoNome));
                return;
            }
        }

        private bool ExisteNomeDuplicado(string nome)
        {
            IEnumerable<TipoTextoAnotativo> tipos = _tiposTemporarios != null ? _tiposTemporarios : new[] { TipoTexto };
            return tipos.Any(t => !ReferenceEquals(t, TipoTexto) && string.Equals(t.NomeTipo?.Trim(), nome, StringComparison.OrdinalIgnoreCase));
        }

        private string GerarNomeUnico(string nomeBase)
        {
            string baseLimpa = string.IsNullOrWhiteSpace(nomeBase) ? "Texto" : nomeBase.Trim();
            IEnumerable<TipoTextoAnotativo> tipos = _tiposTemporarios != null ? _tiposTemporarios : new[] { TipoTexto };
            var existentes = tipos
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

        private void AtualizarListaDeTipos()
        {
            if (_tiposTemporarios != null)
                CollectionViewSource.GetDefaultView(_tiposTemporarios)?.Refresh();

            OnPropertyChanged(nameof(TiposDisponiveis));
            OnPropertyChanged(nameof(TipoSelecionadoNome));
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

        private void NotificarTudo()
        {
            OnPropertyChanged(nameof(TipoSelecionado));
            OnPropertyChanged(nameof(TipoSelecionadoNome));
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

        private static TipoTextoAnotativo ClonarTipo(TipoTextoAnotativo origem)
        {
            return new TipoTextoAnotativo
            {
                NomeTipo = origem.NomeTipo,
                Familia = origem.Familia,
                Categoria = origem.Categoria,
                CorTexto = origem.CorTexto,
                Fonte = origem.Fonte,
                AlturaTexto = origem.AlturaTexto,
                AlinhamentoHorizontal = origem.AlinhamentoHorizontal
            };
        }

        private static void CopiarValores(TipoTextoAnotativo origem, TipoTextoAnotativo destino)
        {
            destino.NomeTipo = origem.NomeTipo;
            destino.Familia = origem.Familia;
            destino.Categoria = origem.Categoria;
            destino.CorTexto = origem.CorTexto;
            destino.Fonte = origem.Fonte;
            destino.AlturaTexto = origem.AlturaTexto;
            destino.AlinhamentoHorizontal = origem.AlinhamentoHorizontal;
        }

        private static string NormalizarNome(string? nome)
        {
            return string.IsNullOrWhiteSpace(nome) ? string.Empty : nome.Trim();
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
