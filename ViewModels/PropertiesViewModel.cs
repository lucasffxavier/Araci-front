using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Applications.UseCases.Editar;
using Araci.Models.Tipos;
using Araci.Properties;
using Araci.Services;
using Araci.Services.Settings;

namespace Araci.ViewModels
{
    public class PropertiesViewModel : INotifyPropertyChanged
    {
        private readonly IReadOnlyList<ElementoViewModel> _selecionados;
        private readonly EditarPropriedadesUseCase? _editarPropriedades;
        private readonly EditorSettings _settings;
        private object? _elementoSelecionado;
        private SimpleCommand? _abrirPropriedadesTipoCommand;

        public PropertiesViewModel()
        {
            _selecionados = Array.Empty<ElementoViewModel>();
            _settings = new EditorSettings();
        }

        public PropertiesViewModel(IEnumerable<ElementoViewModel> selecionados, EditarPropriedadesUseCase? editarPropriedades = null, EditorSettings? settings = null)
        {
            _selecionados = selecionados?.Where(e => e != null).ToList() ?? new List<ElementoViewModel>();
            _editarPropriedades = editarPropriedades;
            _settings = settings ?? new EditorSettings();
            QuantidadeSelecionada = _selecionados.Count;
            Titulo = CriarTitulo(_selecionados);
            Propriedades = new ObservableCollection<PropertyDescriptorViewModel>(CriarDescritores(_selecionados, _editarPropriedades, _settings));
            AssinarAlteracoesDosElementosSelecionados();
        }

        public object? ElementoSelecionado
        {
            get => _elementoSelecionado;
            set
            {
                if (_elementoSelecionado == value)
                    return;

                _elementoSelecionado = value;
                OnPropertyChanged();
            }
        }

        public int QuantidadeSelecionada { get; }
        public string Titulo { get; } = "Propriedades";
        public ObservableCollection<PropertyDescriptorViewModel> Propriedades { get; } = new();
        public bool MesmoTipo => _selecionados.Count > 0 && _selecionados.All(e => e.GetType() == _selecionados[0].GetType());
        public bool ExibirSeletorTipo => MesmoTipo;
        public IEnumerable TiposDisponiveis => MesmoTipo ? _selecionados[0].TiposDisponiveis : Array.Empty<object>();
        public bool PodeAbrirPropriedadesTipo => MesmoTipo && Tipo != null;

        public TipoElemento? Tipo
        {
            get
            {
                if (!MesmoTipo || _selecionados.Count == 0)
                    return null;

                var primeiro = _selecionados[0].Tipo;
                return _selecionados.All(e => ReferenceEquals(e.Tipo, primeiro)) ? primeiro : null;
            }
            set
            {
                if (!MesmoTipo || value == null)
                    return;

                if (_editarPropriedades == null)
                    return;

                if (!_editarPropriedades.Executar(_selecionados, nameof(ElementoViewModel.Tipo), value))
                    return;

                AtualizarSeletorTipo();
            }
        }

        public ICommand AbrirPropriedadesTipoCommand => _abrirPropriedadesTipoCommand ??= new SimpleCommand(AbrirPropriedadesTipo, () => PodeAbrirPropriedadesTipo);

        private void AbrirPropriedadesTipo()
        {
            if (!PodeAbrirPropriedadesTipo)
                return;

            _selecionados[0].AbrirPropriedadesTipoCommand.Execute(null);
            AtualizarSeletorTipo();
        }

        private void AssinarAlteracoesDosElementosSelecionados()
        {
            foreach (ElementoViewModel elemento in _selecionados)
                elemento.PropertyChanged += OnElementoSelecionadoPropertyChanged;
        }

        private void OnElementoSelecionadoPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ElementoViewModel.Tipo) || e.PropertyName == nameof(ElementoViewModel.TipoViewModel))
                AtualizarSeletorTipo();
        }

        private void AtualizarSeletorTipo()
        {
            OnPropertyChanged(nameof(TiposDisponiveis));
            OnPropertyChanged(nameof(Tipo));
            OnPropertyChanged(nameof(PodeAbrirPropriedadesTipo));
            _abrirPropriedadesTipoCommand?.RaiseCanExecuteChanged();
        }

        private static string CriarTitulo(IReadOnlyList<ElementoViewModel> itens)
        {
            if (itens.Count == 0)
                return "Nenhum elemento selecionado";

            if (itens.All(i => i.GetType() == itens[0].GetType()))
                return CriarTituloMesmoTipo(itens[0], itens.Count);

            return $"{itens.Count} elementos selecionados";
        }

        private static string CriarTituloMesmoTipo(ElementoViewModel item, int quantidade)
        {
            string tipo = ObterNomeTipoAmigavel(item, quantidade > 1);
            string selecionado = ObterParticipioSelecionado(item, quantidade > 1);

            return $"{quantidade} {tipo} {selecionado}";
        }

        private static IEnumerable<PropertyDescriptorViewModel> CriarDescritores(IReadOnlyList<ElementoViewModel> itens, EditarPropriedadesUseCase? editarPropriedades, EditorSettings settings)
        {
            if (itens.Count == 0)
                yield break;

            bool mesmoTipo = itens.All(i => i.GetType() == itens[0].GetType());

            foreach (var descriptor in InstancePropertyCatalog.GetCommonFor(itens))
            {
                var props = itens.Select(i => ObterPropriedade(i, descriptor.PropertyName)).ToList();

                if (props.Any(p => p == null))
                    continue;

                Type tipoValor = props[0]!.PropertyType;
                bool editavelMesmoTipo = mesmoTipo && descriptor.IsEditable;
                bool editavelTiposDiferentes = !mesmoTipo && InstancePropertyCatalog.CanEditAcrossMixedTypes(itens, descriptor.PropertyName);
                bool editavel = (editavelMesmoTipo || editavelTiposDiferentes) && props.All(p => p != null && p.CanWrite && EhTipoEditavel(p.PropertyType)) && TiposCompativeis(props);
                var valores = itens.Select(i => ObterValor(i, descriptor.PropertyName)).ToList();
                object? primeiro = valores[0];
                bool varia = valores.Skip(1).Any(v => !ValoresIguais(primeiro, v));

                yield return new PropertyDescriptorViewModel(itens, descriptor, tipoValor, varia, editavel, editarPropriedades, settings);
            }
        }

        private static bool TiposCompativeis(IReadOnlyList<PropertyInfo?> props)
        {
            if (props.Count == 0 || props[0] == null)
                return false;

            Type primeiro = Nullable.GetUnderlyingType(props[0]!.PropertyType) ?? props[0]!.PropertyType;

            foreach (var prop in props.Skip(1))
            {
                if (prop == null)
                    return false;

                Type atual = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (atual != primeiro)
                    return false;
            }

            return true;
        }

        private static PropertyInfo? ObterPropriedade(ElementoViewModel item, string nomePropriedade)
        {
            var prop = item.GetType().GetProperty(nomePropriedade, BindingFlags.Instance | BindingFlags.Public);
            return prop == null || prop.GetIndexParameters().Length > 0 ? null : prop;
        }

        internal static object? ObterValor(ElementoViewModel item, string nomePropriedade)
        {
            return ObterPropriedade(item, nomePropriedade)?.GetValue(item);
        }

        internal static bool ValoresIguais(object? a, object? b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            if (a is double da && b is double db)
                return Math.Abs(da - db) < 0.000001;

            if (a is float fa && b is float fb)
                return Math.Abs(fa - fb) < 0.000001f;

            return Equals(a, b);
        }

        internal static string FormatarValor(object? valor)
        {
            return UnitFormatter.Format(valor, UnitKind.None);
        }

        internal static string FormatarValor(object? valor, InstancePropertyDescriptor descriptor)
        {
            return UnitFormatter.Format(valor, descriptor.Unit);
        }

        internal static string FormatarValor(object? valor, UnitKind baseUnit, UnitKind displayUnit)
        {
            return UnitFormatter.Format(valor, baseUnit, displayUnit);
        }

        internal static bool TentarConverterValor(string valor, Type tipoDestino, out object? convertido)
        {
            return TentarConverterValor(valor, tipoDestino, UnitKind.None, out convertido);
        }

        internal static bool TentarConverterValor(string valor, Type tipoDestino, InstancePropertyDescriptor descriptor, out object? convertido)
        {
            return TentarConverterValor(valor, tipoDestino, descriptor.Unit, out convertido);
        }

        internal static bool TentarConverterValor(string valor, Type tipoDestino, UnitKind baseUnit, UnitKind displayUnit, out object? convertido)
        {
            return TentarConverterValorConvertendo(valor, tipoDestino, displayUnit, baseUnit, out convertido);
        }

        internal static bool TentarConverterValor(string valor, Type tipoDestino, UnitKind unit, out object? convertido)
        {
            return TentarConverterValorConvertendo(valor, tipoDestino, unit, unit, out convertido);
        }

        private static bool TentarConverterValorConvertendo(string valor, Type tipoDestino, UnitKind displayUnit, UnitKind baseUnit, out object? convertido)
        {
            convertido = null;
            Type tipo = Nullable.GetUnderlyingType(tipoDestino) ?? tipoDestino;
            string valorSemUnidade = UnitFormatter.StripUnit(valor, displayUnit);

            try
            {
                if (tipo == typeof(string))
                {
                    convertido = valorSemUnidade;
                    return true;
                }

                if (tipo == typeof(int))
                {
                    if (!int.TryParse(valorSemUnidade, NumberStyles.Integer, CultureInfo.CurrentCulture, out int i))
                        return false;

                    convertido = i;
                    return true;
                }

                if (tipo == typeof(double))
                {
                    if (!double.TryParse(valorSemUnidade, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out double d))
                        return false;

                    convertido = UnitFormatter.FromDisplay(d, displayUnit, baseUnit);
                    return true;
                }

                if (tipo == typeof(float))
                {
                    if (!float.TryParse(valorSemUnidade, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out float f))
                        return false;

                    convertido = (float)UnitFormatter.FromDisplay(f, displayUnit, baseUnit);
                    return true;
                }

                if (tipo == typeof(decimal))
                {
                    if (!decimal.TryParse(valorSemUnidade, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out decimal m))
                        return false;

                    convertido = (decimal)UnitFormatter.FromDisplay((double)m, displayUnit, baseUnit);
                    return true;
                }

                if (tipo == typeof(bool))
                {
                    if (TentarConverterBooleano(valorSemUnidade, out bool b))
                    {
                        convertido = b;
                        return true;
                    }

                    return false;
                }

                if (tipo.IsEnum)
                {
                    convertido = Enum.Parse(tipo, valorSemUnidade, true);
                    return true;
                }

                convertido = Convert.ChangeType(valorSemUnidade, tipo, CultureInfo.CurrentCulture);
                return true;
            }
            catch
            {
                convertido = null;
                return false;
            }
        }

        private static bool TentarConverterBooleano(string valor, out bool convertido)
        {
            convertido = false;

            if (bool.TryParse(valor, out bool b))
            {
                convertido = b;
                return true;
            }

            string normalizado = (valor ?? string.Empty).Trim().ToLowerInvariant();

            if (normalizado is "sim" or "s" or "yes" or "y" or "1")
            {
                convertido = true;
                return true;
            }

            if (normalizado is "não" or "nao" or "n" or "no" or "0")
            {
                convertido = false;
                return true;
            }

            return false;
        }

        private static bool EhTipoEditavel(Type tipo)
        {
            Type t = Nullable.GetUnderlyingType(tipo) ?? tipo;
            return t == typeof(string) || t == typeof(int) || t == typeof(double) || t == typeof(float) || t == typeof(decimal) || t == typeof(bool) || t.IsEnum;
        }

        private static string ObterNomeTipoAmigavel(ElementoViewModel vm, bool plural)
        {
            if (vm is LinhaAnotativaViewModel)
                return plural ? "Linhas" : "Linha";

            if (vm is TextoAnotativoViewModel)
                return plural ? "Textos" : "Texto";

            if (vm is RetanguloAnotativoViewModel)
                return plural ? "Retângulos" : "Retângulo";

            if (vm is CirculoAnotativoViewModel)
                return plural ? "Círculos" : "Círculo";

            return vm.GetType().Name.Replace("ViewModel", string.Empty);
        }

        private static string ObterParticipioSelecionado(ElementoViewModel vm, bool plural)
        {
            if (vm is LinhaAnotativaViewModel)
                return plural ? "selecionadas" : "selecionada";

            return plural ? "selecionados" : "selecionado";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
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

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public class PropertyDescriptorViewModel : INotifyPropertyChanged
    {
        private readonly IReadOnlyList<ElementoViewModel> _elementos;
        private readonly InstancePropertyDescriptor _descriptor;
        private readonly Type _tipoValor;
        private readonly EditarPropriedadesUseCase? _editarPropriedades;
        private ICommand? _escolherCorCommand;
        private string _valor;
        private bool _varia;
        private bool _temErro;
        private string _mensagemErro = string.Empty;
        private static readonly IReadOnlyList<BooleanPropertyOption> _booleanOptions = new[]
        {
            new BooleanPropertyOption(true, "Sim"),
            new BooleanPropertyOption(false, "Não")
        };

        public PropertyDescriptorViewModel(IReadOnlyList<ElementoViewModel> elementos, InstancePropertyDescriptor descriptor, Type tipoValor, bool varia, bool isEditable, EditarPropriedadesUseCase? editarPropriedades = null)
            : this(elementos, descriptor, tipoValor, varia, isEditable, editarPropriedades, null)
        {
        }

        public PropertyDescriptorViewModel(IReadOnlyList<ElementoViewModel> elementos, InstancePropertyDescriptor descriptor, Type tipoValor, bool varia, bool isEditable, EditarPropriedadesUseCase? editarPropriedades, EditorSettings? settings)
        {
            _elementos = elementos;
            _descriptor = descriptor;
            _tipoValor = tipoValor;
            _editarPropriedades = editarPropriedades;
            Nome = descriptor.DisplayName;
            DisplayName = descriptor.DisplayName;
            PropertyName = descriptor.PropertyName;
            BaseUnit = descriptor.Unit;
            DisplayUnit = ResolveDisplayUnit(BaseUnit, settings);
            Unit = DisplayUnit;
            UnitSymbol = UnitFormatter.GetSymbol(DisplayUnit);
            IsEditable = isEditable;
            IsReadOnly = !isEditable;
            IsColor = descriptor.IsColor;
            _varia = varia;
            _valor = varia ? "<varia>" : FormatarValorLocal(ObterValorAtual());
            AssinarAlteracoesDosElementos();
        }

        public string Nome { get; }
        public string DisplayName { get; }
        public string PropertyName { get; }
        public Type ValueType => _tipoValor;
        public UnitKind BaseUnit { get; }
        public UnitKind DisplayUnit { get; }
        public UnitKind Unit { get; }
        public string UnitSymbol { get; }
        public bool HasUnit => DisplayUnit != UnitKind.None;
        public bool IsEditable { get; }
        public bool IsReadOnly { get; }
        public bool IsColor { get; }
        public bool IsBoolean => (Nullable.GetUnderlyingType(_tipoValor) ?? _tipoValor) == typeof(bool);
        public IReadOnlyList<BooleanPropertyOption> BooleanOptions => _booleanOptions;
        public bool? BooleanValue
        {
            get
            {
                if (_varia)
                    return null;

                object? atual = ObterValorAtual();
                return atual is bool b ? b : null;
            }
            set
            {
                if (!IsEditable || value == null)
                    return;

                AplicarValor(value.Value ? "true" : "false");
            }
        }
        public bool IsMixed => _varia;
        public bool Varia => _varia;
        public Brush ColorBrush => CriarColorBrush(_valor);
        public ICommand? EscolherCorCommand => IsColor
            ? _escolherCorCommand ??= new ColorCommand(EscolherCor, () => IsEditable)
            : null;

        public string Valor
        {
            get => _valor;
            set => AplicarValor(value);
        }

        public string Value
        {
            get => _valor;
            set => AplicarValor(value);
        }

        public bool TemErro
        {
            get => _temErro;
            private set
            {
                if (_temErro == value)
                    return;

                _temErro = value;
                OnPropertyChanged();
            }
        }

        public string MensagemErro
        {
            get => _mensagemErro;
            private set
            {
                if (_mensagemErro == value)
                    return;

                _mensagemErro = value;
                OnPropertyChanged();
            }
        }

        private void AplicarValor(string novoValor)
        {
            if (!IsEditable)
                return;

            if (string.Equals(novoValor, "<varia>", StringComparison.OrdinalIgnoreCase))
                return;

            if (IsColor)
            {
                if (!ColorPickerWindow.TryNormalizeHexColor(novoValor, out string corNormalizada))
                {
                    AtualizarValorInvalido(novoValor);
                    return;
                }

                novoValor = corNormalizada;
            }

            if (string.Equals(novoValor, _valor, StringComparison.Ordinal))
                return;

            if (!PropertiesViewModel.TentarConverterValor(novoValor, _tipoValor, BaseUnit, DisplayUnit, out object? convertido))
            {
                AtualizarValorInvalido(novoValor);
                return;
            }

            bool aplicado = _editarPropriedades?.Executar(_elementos, _descriptor.PropertyName, convertido) ?? false;

            if (aplicado)
                AtualizarValorDoModelo();
            else
                AtualizarValorLocal(convertido);
        }

        private void AtualizarValorInvalido(string valor)
        {
            TemErro = true;
            MensagemErro = "Valor inválido";
            _valor = valor;
            OnPropertyChanged(nameof(Valor));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(ColorBrush));
            OnPropertyChanged(nameof(BooleanValue));
        }

        private void AtualizarValorLocal(object? convertido)
        {
            _varia = false;
            _valor = FormatarValorLocal(convertido);
            AtualizarEstadoValido();
        }

        private void AtualizarValorDoModelo()
        {
            if (_elementos.Count == 0)
                return;

            var valores = _elementos.Select(e => PropertiesViewModel.ObterValor(e, _descriptor.PropertyName)).ToList();
            object? primeiro = valores[0];
            bool varia = valores.Skip(1).Any(v => !PropertiesViewModel.ValoresIguais(primeiro, v));
            string valorAtualizado = varia ? "<varia>" : FormatarValorLocal(primeiro);

            if (_varia == varia && string.Equals(_valor, valorAtualizado, StringComparison.Ordinal))
            {
                AtualizarEstadoValido();
                return;
            }

            _varia = varia;
            _valor = valorAtualizado;
            AtualizarEstadoValido();
        }

        private string FormatarValorLocal(object? convertido)
        {
            if (IsColor && convertido is string cor && ColorPickerWindow.TryNormalizeHexColor(cor, out string corNormalizada))
                return corNormalizada;

            if (IsBoolean && convertido is bool b)
                return b ? "Sim" : "Não";

            return PropertiesViewModel.FormatarValor(convertido, BaseUnit, DisplayUnit);
        }

        private static UnitKind ResolveDisplayUnit(UnitKind baseUnit, EditorSettings? settings)
        {
            if (baseUnit == UnitKind.None)
                return UnitKind.None;

            UnitQuantityKind quantity = UnitFormatter.GetQuantity(baseUnit);
            return settings?.Units.Resolve(quantity) ?? baseUnit;
        }

        private void AtualizarEstadoValido()
        {
            TemErro = false;
            MensagemErro = string.Empty;
            OnPropertyChanged(nameof(Valor));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(ColorBrush));
            OnPropertyChanged(nameof(BooleanValue));
            OnPropertyChanged(nameof(IsMixed));
            OnPropertyChanged(nameof(Varia));
        }

        private void EscolherCor()
        {
            var window = new ColorPickerWindow(_valor)
            {
                Owner = Application.Current?.MainWindow
            };

            if (window.ShowDialog() == true)
                Valor = window.SelectedColorHex;
        }

        private void AssinarAlteracoesDosElementos()
        {
            foreach (ElementoViewModel elemento in _elementos)
                elemento.PropertyChanged += OnElementoPropertyChanged;
        }

        private void OnElementoPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != _descriptor.PropertyName)
                return;

            AtualizarValorDoModelo();
        }

        private static Brush CriarColorBrush(string value)
        {
            try
            {
                if (ColorPickerWindow.TryNormalizeColor(value, out string normalized) &&
                    ColorConverter.ConvertFromString(normalized) is Color color)
                {
                    return new SolidColorBrush(color);
                }
            }
            catch (FormatException)
            {
            }

            return Brushes.Black;
        }

        private object? ObterValorAtual()
        {
            if (_elementos.Count == 0)
                return null;

            return PropertiesViewModel.ObterValor(_elementos[0], _descriptor.PropertyName);
        }

        private sealed class ColorCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool>? _canExecute;

            public ColorCommand(Action execute, Func<bool>? canExecute = null)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }

    public sealed class BooleanPropertyOption
    {
        public BooleanPropertyOption(bool value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public bool Value { get; }
        public string DisplayName { get; }
    }

    public class PropertyRowViewModel : PropertyDescriptorViewModel
    {
        public PropertyRowViewModel(string nome, string valor, bool varia)
            : base(Array.Empty<ElementoViewModel>(), new InstancePropertyDescriptor(typeof(ElementoViewModel), string.Empty, nome, 0, false), typeof(string), varia, false)
        {
        }
    }
}