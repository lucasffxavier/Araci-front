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
            Propriedades = new ObservableCollection<PropertyRowViewModel>(CriarLinhas(itens));
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
        public ObservableCollection<PropertyRowViewModel> Propriedades { get; } = new();

        private static string CriarTitulo(IReadOnlyList<ElementoViewModel> itens)
        {
            if (itens.Count == 0)
                return "Nenhum elemento selecionado";

            string tipo = ObterNomeTipoAmigavel(itens[0]);

            if (itens.All(i => i.GetType() == itens[0].GetType()))
                return $"{itens.Count} {tipo}{(itens.Count > 1 ? "s" : "")} selecionados";

            return $"{itens.Count} elementos selecionados";
        }

        private static IEnumerable<PropertyRowViewModel> CriarLinhas(IReadOnlyList<ElementoViewModel> itens)
        {
            if (itens.Count == 0)
                yield break;

            foreach (string nomePropriedade in ObterNomesComunsExibiveis(itens))
            {
                var valores = itens.Select(i => ObterValor(i, nomePropriedade)).ToList();
                object? primeiro = valores[0];
                bool varia = valores.Skip(1).Any(v => !ValoresIguais(primeiro, v));
                string valor = varia ? "<varia>" : FormatarValor(primeiro);
                yield return new PropertyRowViewModel(FormatarNome(nomePropriedade), valor, varia);
            }
        }

        private static IEnumerable<string> ObterNomesComunsExibiveis(IReadOnlyList<ElementoViewModel> itens)
        {
            HashSet<string>? comuns = null;

            foreach (ElementoViewModel item in itens)
            {
                var nomes = ObterNomesExibiveis(item).ToHashSet();

                if (comuns == null)
                {
                    comuns = nomes;
                    continue;
                }

                comuns.IntersectWith(nomes);
            }

            return Ordenar(comuns ?? new HashSet<string>());
        }

        private static IEnumerable<string> ObterNomesExibiveis(ElementoViewModel item)
        {
            return item switch
            {
                BarraViewModel => new[] { "Nome", "Tensao", "Altura" },
                CaboViewModel => new[] { "Nome", "BarraOrigem", "BarraDestino", "Comprimento", "Ampacidade", "TensaoLinha", "TensaoFaseA", "TensaoFaseB", "TensaoFaseC", "CorrenteLinha", "CorrenteFaseA", "CorrenteFaseB", "CorrenteFaseC" },
                CargaViewModel => new[] { "Nome", "PotenciaAtiva", "PotenciaReativa", "Alimentador", "CorrenteLinha", "CorrenteFaseA", "CorrenteFaseB", "CorrenteFaseC", "TensaoLinha", "TensaoFaseA", "TensaoFaseB", "TensaoFaseC" },
                GeradorViewModel => new[] { "Nome", "PotenciaAparente", "PotenciaAtiva", "PotenciaReativa", "TensaoLinha", "TensaoFaseA", "TensaoFaseB", "TensaoFaseC", "CorrenteLinha", "CorrenteFaseA", "CorrenteFaseB", "CorrenteFaseC" },
                SinViewModel => new[] { "Nome", "TensaoLinha" },
                TransformadorViewModel => new[] { "Nome", "Alimentador", "Fases", "Enrolamentos", "TensaoPrimarioKV", "TensaoSecundarioKV", "PotenciaAparente", "RPercentual", "XPercentual", "LigacaoPrimario", "LigacaoSecundario" },
                _ => Array.Empty<string>()
            };
        }

        private static object? ObterValor(ElementoViewModel item, string nomePropriedade)
        {
            PropertyInfo? prop = item.GetType().GetProperty(nomePropriedade, BindingFlags.Instance | BindingFlags.Public);
            return prop == null || prop.GetIndexParameters().Length > 0 ? null : prop.GetValue(item);
        }

        private static IEnumerable<string> Ordenar(IEnumerable<string> propriedades)
        {
            string[] prioridade =
            {
                "Nome",
                "BarraOrigem",
                "BarraDestino",
                "Alimentador",
                "Fases",
                "Enrolamentos",
                "Tensao",
                "TensaoLinha",
                "TensaoPrimarioKV",
                "TensaoSecundarioKV",
                "PotenciaAparente",
                "PotenciaAtiva",
                "PotenciaReativa",
                "Comprimento",
                "Ampacidade",
                "RPercentual",
                "XPercentual",
                "LigacaoPrimario",
                "LigacaoSecundario"
            };

            return propriedades
                .OrderBy(p =>
                {
                    int index = Array.IndexOf(prioridade, p);
                    return index >= 0 ? index : 1000;
                })
                .ThenBy(p => p);
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

        private static string FormatarValor(object? valor)
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

        private static string FormatarNome(string nome)
        {
            return nome switch
            {
                "Tensao" => "Tensão",
                "TensaoLinha" => "Tensão linha",
                "TensaoFaseA" => "Tensão fase A",
                "TensaoFaseB" => "Tensão fase B",
                "TensaoFaseC" => "Tensão fase C",
                "TensaoPrimarioKV" => "Tensão primário",
                "TensaoSecundarioKV" => "Tensão secundário",
                "PotenciaAparente" => "Potência aparente",
                "PotenciaAtiva" => "Potência ativa",
                "PotenciaReativa" => "Potência reativa",
                "CorrenteLinha" => "Corrente linha",
                "CorrenteFaseA" => "Corrente fase A",
                "CorrenteFaseB" => "Corrente fase B",
                "CorrenteFaseC" => "Corrente fase C",
                "BarraOrigem" => "Barra origem",
                "BarraDestino" => "Barra destino",
                "RPercentual" => "R",
                "XPercentual" => "X",
                "LigacaoPrimario" => "Ligação primário",
                "LigacaoSecundario" => "Ligação secundário",
                _ => SepararCamelCase(nome)
            };
        }

        private static string SepararCamelCase(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return string.Empty;

            var resultado = new List<char>();

            for (int i = 0; i < nome.Length; i++)
            {
                char c = nome[i];

                if (i > 0 && char.IsUpper(c) && !char.IsUpper(nome[i - 1]))
                    resultado.Add(' ');

                resultado.Add(c);
            }

            return new string(resultado.ToArray());
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

    public class PropertyRowViewModel
    {
        public PropertyRowViewModel(string nome, string valor, bool varia)
        {
            Nome = nome;
            Valor = valor;
            Varia = varia;
        }

        public string Nome { get; }
        public string Valor { get; }
        public bool Varia { get; }
    }
}