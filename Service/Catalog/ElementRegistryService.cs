using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Core.Rendering;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.ViewModels;

namespace Araci.Services.Catalog
{
    public class ElementRegistryService : IElementCatalog
    {
        public const string KindBarra = ElementKinds.Barra;
        public const string KindCarga = ElementKinds.Carga;
        public const string KindGerador = ElementKinds.Gerador;
        public const string KindSin = ElementKinds.Sin;
        public const string KindTransformador = ElementKinds.Transformador;
        public const string KindCabo = ElementKinds.Cabo;

        private readonly Dictionary<string, ElementDefinition> _porKind = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<ElementDefinition> _definitions = new();

        public ElementRegistryService(IEnumerable<ElementDefinition> definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            foreach (ElementDefinition definition in definitions)
                Register(definition);
        }

        public IReadOnlyList<ElementDefinition> Definitions => _definitions;
        public IEnumerable<ElementDefinition> RibbonDefinitions => _definitions
            .Where(d => d.ExibirNoRibbon)
            .OrderBy(d => d.OrdemRibbon)
            .ThenBy(d => d.NomeRibbon);

        public void Register(ElementDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (_porKind.ContainsKey(definition.Kind))
                throw new InvalidOperationException($"Elemento ja registrado: {definition.Kind}.");

            _porKind[definition.Kind] = definition;
            _definitions.Add(definition);
        }

        public ElementDefinition? FindByKind(string kind)
        {
            return string.IsNullOrWhiteSpace(kind)
                ? null
                : _porKind.TryGetValue(kind.Trim(), out ElementDefinition? definition) ? definition : null;
        }

        public ElementDefinition? FindByShortcut(string shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut))
                return null;

            string normalized = shortcut.Trim().ToUpperInvariant();
            return _definitions.FirstOrDefault(d => string.Equals(d.Atalho, normalized, StringComparison.OrdinalIgnoreCase));
        }

        public ElementDefinition? FindByModel(Elemento elemento)
        {
            return _definitions.FirstOrDefault(d => d.AceitaModelo(elemento));
        }

        public ElementDefinition? FindByModelType<T>() where T : Elemento
        {
            return _definitions.FirstOrDefault(d => d.ModelType == typeof(T));
        }

        public ElementDefinition? FindByViewModel(ElementoViewModel viewModel)
        {
            return _definitions.FirstOrDefault(d => d.AceitaViewModel(viewModel));
        }

        public ElementDefinition? FindByViewModelType(Type viewModelType)
        {
            return _definitions.FirstOrDefault(d => d.ViewModelType == viewModelType);
        }

        public string GetKind(Elemento elemento)
        {
            return FindByModel(elemento)?.Kind ?? elemento.GetType().Name;
        }

        public string GetNamePrefix(Elemento elemento)
        {
            return FindByModel(elemento)?.PrefixoNome ?? "ELM";
        }

        public IEnumerable<TipoElemento> GetTypes(string kind)
        {
            return FindByKind(kind)?.ObterTipos() ?? Enumerable.Empty<TipoElemento>();
        }

        public TipoElemento? GetDefaultType(string kind)
        {
            return FindByKind(kind)?.ObterTipoPadrao();
        }

        public TipoElemento? ResolveType(string kind, string? nomeTipo, string? familia, string? categoria)
        {
            TipoElemento? tipo = null;

            if (!string.IsNullOrWhiteSpace(nomeTipo))
            {
                tipo = GetTypes(kind).FirstOrDefault(t =>
                    string.Equals(t.NomeTipo, nomeTipo, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Familia, familia, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Categoria, categoria, StringComparison.OrdinalIgnoreCase));
            }

            return tipo ?? GetDefaultType(kind);
        }

        public Size GetSize(Elemento elemento)
        {
            ElementDefinition? definition = FindByModel(elemento);
            return definition?.ObterTamanho(elemento) ?? GetFallbackSize(elemento);
        }

        public bool UpdateTerminals(Elemento elemento)
        {
            ElementDefinition? definition = FindByModel(elemento);

            if (definition == null)
                return false;

            definition.AtualizarTerminais(elemento);
            return true;
        }

        public IReadOnlyList<InstancePropertyDescriptor> GetInstanceProperties(ElementoViewModel elemento)
        {
            return FindByViewModel(elemento)?.PropriedadesInstancia ?? Array.Empty<InstancePropertyDescriptor>();
        }

        public IReadOnlyList<InstancePropertyDescriptor> GetInstanceProperties(Type viewModelType)
        {
            return FindByViewModelType(viewModelType)?.PropriedadesInstancia ?? Array.Empty<InstancePropertyDescriptor>();
        }

        public IReadOnlyList<InstancePropertyDescriptor> GetCommonInstanceProperties(IReadOnlyList<ElementoViewModel> elementos)
        {
            if (elementos.Count == 0)
                return Array.Empty<InstancePropertyDescriptor>();

            var commonNames = new HashSet<string>(GetInstanceProperties(elementos[0]).Select(p => p.PropertyName));

            foreach (ElementoViewModel elemento in elementos.Skip(1))
                commonNames.IntersectWith(GetInstanceProperties(elemento).Select(p => p.PropertyName));

            return GetInstanceProperties(elementos[0])
                .Where(p => commonNames.Contains(p.PropertyName))
                .OrderBy(p => p.Order)
                .ThenBy(p => p.DisplayName)
                .ToList();
        }

        public bool CanEditAcrossMixedTypes(IReadOnlyList<ElementoViewModel> elementos, string propertyName)
        {
            if (elementos.Count == 0 || string.IsNullOrWhiteSpace(propertyName))
                return false;

            return elementos.All(e => GetInstanceProperties(e).Any(p => p.PropertyName == propertyName && p.IsEditable && p.AllowMixedTypeEdit));
        }

        private static Size GetFallbackSize(Elemento elemento)
        {
            return elemento switch
            {
                Barra barra => new Size(ElementGeometryDefaults.BarraLargura, barra.Altura),
                ElementoEquipamento => new Size(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                ElementoLinear => Size.Empty,
                _ => new Size(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura)
            };
        }
    }
}
