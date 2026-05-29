using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Araci.ViewModels
{
    public class PropertiesViewModel : INotifyPropertyChanged
    {
        private object? _elementoSelecionado;

        public PropertiesViewModel()
        {
        }

        public PropertiesViewModel(IEnumerable<ElementoViewModel> selecionados)
        {
            var itens = selecionados?.Where(e => e != null).ToList() ?? new List<ElementoViewModel>();
            QuantidadeSelecionada = itens.Count;
            Titulo = CriarTitulo(itens);
            Propriedades = new ObservableCollection<PropertyDescriptorViewModel>(CriarDescritores(itens));
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

        private static string CriarTitulo(IReadOnlyList<ElementoViewModel> itens)
        {
            if (itens.Count == 0)
                return "Nenhum elemento selecionado";

            string tipo = ObterNomeTipoAmigavel(itens[0]);

            if (itens.All(i => i.GetType() == itens[0].GetType()))
                return $"{itens.Count} {tipo}{(itens.Count > 1 ? "s" : "")} selecionados";

            return $"{itens.Count} elementos selecionados";
        }

        private static IEnumerable<PropertyDescriptorViewModel> CriarDescritores(IReadOnlyList<ElementoViewModel> itens)
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
                bool editavel = mesmoTipo && descriptor.IsEditable && props.All(p => p != null && p.CanWrite && EhTipoEditavel(p.PropertyType));
                var valores = itens.Select(i => ObterValor(i, descriptor.PropertyName)).ToList();
                object? primeiro = valores[0];
                bool varia = valores.Skip(1).Any(v => !ValoresIguais(primeiro, v));

                yield return new PropertyDescriptorViewModel(itens, descriptor, tipoValor, varia, editavel);
            }
        }

        private static PropertyInfo? ObterPropriedade(ElementoViewModel item, string nomePropriedade)
        {
            var prop = item.GetType().GetProperty(nomePropriedade, BindingFlags.Instance | BindingFlags.Public);
            return prop == null || prop.GetIndexParameters().Length > 0 ? null : prop;
        }

        private static object? ObterValor(ElementoViewModel item, string nomePropriedade)
        {
            return ObterPropriedade(item, nomePropriedade)?.GetValue(item);
        }

        private static bool ValoresIguais(object? a, object? b)
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
            if (valor == null)
                return string.Empty;

            return valor switch
            {
                double d => d.ToString("N2", CultureInfo.CurrentCulture),
                float f => f.ToString("N2", CultureInfo.CurrentCulture),
                decimal m => m.ToString("N2", CultureInfo.CurrentCulture),
                _ => valor.ToString() ?? string.Empty
            };
        }

        internal static bool TentarConverterValor(string valor, Type tipoDestino, out object? convertido)
        {
            convertido = null;
            Type tipo = Nullable.GetUnderlyingType(tipoDestino) ?? tipoDestino;

            try
            {
                if (tipo == typeof(string))
                {
                    convertido = valor;
                    return true;
                }

                if (tipo == typeof(int))
                {
                    if (!int.TryParse(valor, NumberStyles.Integer, CultureInfo.CurrentCulture, out int i))
                        return false;

                    convertido = i;
                    return true;
                }

                if (tipo == typeof(double))
                {
                    if (!double.TryParse(valor, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out double d))
                        return false;

                    convertido = d;
                    return true;
                }

                if (tipo == typeof(float))
                {
                    if (!float.TryParse(valor, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out float f))
                        return false;

                    convertido = f;
                    return true;
                }

                if (tipo == typeof(decimal))
                {
                    if (!decimal.TryParse(valor, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out decimal m))
                        return false;

                    convertido = m;
                    return true;
                }

                if (tipo == typeof(bool))
                {
                    if (!bool.TryParse(valor, out bool b))
                        return false;

                    convertido = b;
                    return true;
                }

                if (tipo.IsEnum)
                {
                    convertido = Enum.Parse(tipo, valor, true);
                    return true;
                }

                convertido = Convert.ChangeType(valor, tipo, CultureInfo.CurrentCulture);
                return true;
            }
            catch
            {
                convertido = null;
                return false;
            }
        }

        private static bool EhTipoEditavel(Type tipo)
        {
            Type t = Nullable.GetUnderlyingType(tipo) ?? tipo;
            return t == typeof(string) || t == typeof(int) || t == typeof(double) || t == typeof(float) || t == typeof(decimal) || t == typeof(bool) || t.IsEnum;
        }

        private static string ObterNomeTipoAmigavel(ElementoViewModel vm)
        {
            return vm.GetType().Name.Replace("ViewModel", string.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }

    public class PropertyDescriptorViewModel : INotifyPropertyChanged
    {
        private readonly IReadOnlyList<ElementoViewModel> _elementos;
        private readonly InstancePropertyDescriptor _descriptor;
        private readonly Type _tipoValor;
        private string _valor;
        private bool _varia;
        private bool _temErro;
        private string _mensagemErro = string.Empty;

        public PropertyDescriptorViewModel(IReadOnlyList<ElementoViewModel> elementos, InstancePropertyDescriptor descriptor, Type tipoValor, bool varia, bool isEditable)
        {
            _elementos = elementos;
            _descriptor = descriptor;
            _tipoValor = tipoValor;
            Nome = descriptor.DisplayName;
            DisplayName = descriptor.DisplayName;
            PropertyName = descriptor.PropertyName;
            IsEditable = isEditable;
            IsReadOnly = !isEditable;
            _varia = varia;
            _valor = varia ? "<varia>" : PropertiesViewModel.FormatarValor(ObterValorAtual());
        }

        public string Nome { get; }
        public string DisplayName { get; }
        public string PropertyName { get; }
        public Type ValueType => _tipoValor;
        public bool IsEditable { get; }
        public bool IsReadOnly { get; }
        public bool IsMixed => _varia;
        public bool Varia => _varia;

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

            if (string.Equals(novoValor, _valor, StringComparison.Ordinal))
                return;

            if (string.Equals(novoValor, "<varia>", StringComparison.OrdinalIgnoreCase))
                return;

            if (!PropertiesViewModel.TentarConverterValor(novoValor, _tipoValor, out object? convertido))
            {
                TemErro = true;
                MensagemErro = "Valor inválido";
                _valor = novoValor;
                OnPropertyChanged(nameof(Valor));
                OnPropertyChanged(nameof(Value));
                return;
            }

            foreach (ElementoViewModel elemento in _elementos)
            {
                PropertyInfo? prop = elemento.GetType().GetProperty(_descriptor.PropertyName, BindingFlags.Instance | BindingFlags.Public);

                if (prop == null || !prop.CanWrite || prop.GetIndexParameters().Length > 0)
                    continue;

                prop.SetValue(elemento, convertido);
            }

            TemErro = false;
            MensagemErro = string.Empty;
            _varia = false;
            _valor = PropertiesViewModel.FormatarValor(convertido);
            OnPropertyChanged(nameof(Valor));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(IsMixed));
            OnPropertyChanged(nameof(Varia));
        }

        private object? ObterValorAtual()
        {
            if (_elementos.Count == 0)
                return null;

            PropertyInfo? prop = _elementos[0].GetType().GetProperty(_descriptor.PropertyName, BindingFlags.Instance | BindingFlags.Public);
            return prop == null || prop.GetIndexParameters().Length > 0 ? null : prop.GetValue(_elementos[0]);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }

    public class PropertyRowViewModel : PropertyDescriptorViewModel
    {
        public PropertyRowViewModel(string nome, string valor, bool varia)
            : base(Array.Empty<ElementoViewModel>(), new InstancePropertyDescriptor(typeof(ElementoViewModel), string.Empty, nome, 0, false), typeof(string), varia, false)
        {
        }
    }
}