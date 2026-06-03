using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.Factories;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.ViewModels;

namespace Araci.Applications.Abstractions
{
    public class ElementDefinition
    {
        public ElementDefinition(
            string kind,
            string nomeAmigavel,
            string prefixoNome,
            Type modelType,
            Type? viewModelType,
            Type? typeModelType,
            Func<Elemento> criarModelo,
            Func<Elemento, ElementViewModelFactoryContext, ElementoViewModel?> criarViewModel,
            Func<TipoElemento?> obterTipoPadrao,
            Func<IEnumerable<TipoElemento>> obterTipos,
            Func<Elemento, Size> obterTamanho,
            Action<Elemento> atualizarTerminais,
            ElementRibbonMetadata ribbon,
            bool usaFerramentaEspecial = false,
            IEnumerable<InstancePropertyDescriptor>? propriedadesInstancia = null)
        {
            Kind = kind ?? throw new ArgumentNullException(nameof(kind));
            NomeAmigavel = nomeAmigavel ?? throw new ArgumentNullException(nameof(nomeAmigavel));
            PrefixoNome = prefixoNome ?? throw new ArgumentNullException(nameof(prefixoNome));
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            ViewModelType = viewModelType;
            TypeModelType = typeModelType;
            CriarModelo = criarModelo ?? throw new ArgumentNullException(nameof(criarModelo));
            CriarViewModel = criarViewModel ?? throw new ArgumentNullException(nameof(criarViewModel));
            ObterTipoPadrao = obterTipoPadrao ?? throw new ArgumentNullException(nameof(obterTipoPadrao));
            ObterTipos = obterTipos ?? throw new ArgumentNullException(nameof(obterTipos));
            ObterTamanho = obterTamanho ?? throw new ArgumentNullException(nameof(obterTamanho));
            AtualizarTerminais = atualizarTerminais ?? throw new ArgumentNullException(nameof(atualizarTerminais));
            Ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
            UsaFerramentaEspecial = usaFerramentaEspecial;
            PropriedadesInstancia = propriedadesInstancia == null
                ? Array.Empty<InstancePropertyDescriptor>()
                : propriedadesInstancia.OrderBy(p => p.Order).ThenBy(p => p.DisplayName).ToList();
        }

        public string Kind { get; }
        public string NomeAmigavel { get; }
        public string PrefixoNome { get; }
        public Type ModelType { get; }
        public Type? ViewModelType { get; }
        public Type? TypeModelType { get; }
        public ElementRibbonMetadata Ribbon { get; }
        public string NomeRibbon => Ribbon.Nome;
        public string CategoriaRibbon => Ribbon.Categoria;
        public string Icone => Ribbon.Icone;
        public int OrdemRibbon => Ribbon.Ordem;
        public bool ExibirNoRibbon => Ribbon.Exibir;
        public string Atalho => Ribbon.Atalho;
        public bool UsaFerramentaEspecial { get; }
        public IReadOnlyList<InstancePropertyDescriptor> PropriedadesInstancia { get; }
        public Func<Elemento> CriarModelo { get; }
        public Func<Elemento, ElementViewModelFactoryContext, ElementoViewModel?> CriarViewModel { get; }
        public Func<TipoElemento?> ObterTipoPadrao { get; }
        public Func<IEnumerable<TipoElemento>> ObterTipos { get; }
        public Func<Elemento, Size> ObterTamanho { get; }
        public Action<Elemento> AtualizarTerminais { get; }

        public bool AceitaModelo(Elemento elemento)
        {
            return ModelType.IsInstanceOfType(elemento);
        }

        public bool AceitaViewModel(ElementoViewModel viewModel)
        {
            return ViewModelType != null && ViewModelType.IsInstanceOfType(viewModel);
        }
    }
}
