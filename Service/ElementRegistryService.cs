using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ElementRegistryService
    {
        private readonly Dictionary<string, ElementDefinition> _porKind =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly List<ElementDefinition> _definitions = new();

        public ElementRegistryService(
            TypeLibraryService types,
            TerminalLayoutService terminalLayout)
        {
            Types = types ?? throw new ArgumentNullException(nameof(types));
            TerminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));

            RegistrarElementosPadrao();
        }

        private TypeLibraryService Types { get; }
        private TerminalLayoutService TerminalLayout { get; }

        public IReadOnlyList<ElementDefinition> Definitions => _definitions;

        public void Register(ElementDefinition definition)
        {
            if (_porKind.ContainsKey(definition.Kind))
                throw new InvalidOperationException($"Elemento ja registrado: {definition.Kind}.");

            _porKind[definition.Kind] = definition;
            _definitions.Add(definition);
        }

        public ElementDefinition? FindByKind(string kind)
        {
            return string.IsNullOrWhiteSpace(kind)
                ? null
                : _porKind.TryGetValue(kind.Trim(), out ElementDefinition? definition)
                    ? definition
                    : null;
        }

        public ElementDefinition? FindByModel(Elemento elemento)
        {
            return _definitions.FirstOrDefault(d => d.AceitaModelo(elemento));
        }

        public string GetKind(Elemento elemento)
        {
            return FindByModel(elemento)?.Kind ?? elemento.GetType().Name;
        }

        public string GetNamePrefix(Elemento elemento)
        {
            return FindByModel(elemento)?.PrefixoNome ?? "ELM";
        }

        public Elemento? CreateModel(string kind)
        {
            return FindByKind(kind)?.CriarModelo();
        }

        public T CreateModel<T>() where T : Elemento
        {
            ElementDefinition? definition =
                _definitions.FirstOrDefault(d => d.ModelType == typeof(T));

            if (definition == null)
                throw new InvalidOperationException($"Elemento nao registrado: {typeof(T).Name}.");

            return (T)definition.CriarModelo();
        }

        public ElementoViewModel? CreateViewModel(
            Elemento modelo,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
        {
            return FindByModel(modelo)?.CriarViewModel(
                modelo,
                names,
                typePropertiesDialogs,
                TerminalLayout);
        }

        public IEnumerable<TipoElemento> GetTypes(string kind)
        {
            return FindByKind(kind)?.ObterTipos()
                ?? Enumerable.Empty<TipoElemento>();
        }

        public TipoElemento? GetDefaultType(string kind)
        {
            return FindByKind(kind)?.ObterTipoPadrao();
        }

        public TipoElemento? ResolveType(
            string kind,
            string? nomeTipo,
            string? familia,
            string? categoria)
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

        private void RegistrarElementosPadrao()
        {
            Register(new ElementDefinition(
                "Barra",
                "Barra",
                "BARRA",
                typeof(Barra),
                typeof(BarraViewModel),
                typeof(TipoBarra),
                CriarBarra,
                (m, n, d, l) => new BarraViewModel((Barra)m, Types, n, d, l),
                () => Types.TipoBarraPadrao,
                () => Types.TiposBarras));

            Register(new ElementDefinition(
                "Carga",
                "Carga",
                "CARGA",
                typeof(Carga),
                typeof(CargaViewModel),
                typeof(TipoCarga),
                CriarCarga,
                (m, n, d, l) => new CargaViewModel((Carga)m, Types, n, d, l),
                () => Types.TipoCargaPadrao,
                () => Types.TiposCargas));

            Register(new ElementDefinition(
                "Gerador",
                "Gerador",
                "GERADOR",
                typeof(Gerador),
                typeof(GeradorViewModel),
                typeof(TipoGerador),
                CriarGerador,
                (m, n, d, l) => new GeradorViewModel((Gerador)m, Types, n, d, l),
                () => Types.TipoGeradorPadrao,
                () => Types.TiposGeradores));

            Register(new ElementDefinition(
                "Cabo",
                "Cabo",
                "CABO",
                typeof(Cabo),
                typeof(CaboViewModel),
                typeof(TipoCabo),
                CriarCabo,
                (m, n, d, l) => new CaboViewModel((Cabo)m, Types, n, d),
                () => Types.TipoCaboPadrao,
                () => Types.TiposCabos));
        }

        private Barra CriarBarra()
        {
            var barra = new Barra
            {
                Tipo = Types.TipoBarraPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de barra cadastrado.")
            };

            TerminalLayout.AtualizarTerminais(barra);
            return barra;
        }

        private Carga CriarCarga()
        {
            var carga = new Carga
            {
                Tipo = Types.TipoCargaPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de carga cadastrado.")
            };

            TerminalLayout.AtualizarTerminais(carga);
            return carga;
        }

        private Gerador CriarGerador()
        {
            var gerador = new Gerador
            {
                Tipo = Types.TipoGeradorPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de gerador cadastrado.")
            };

            TerminalLayout.AtualizarTerminais(gerador);
            return gerador;
        }

        private Cabo CriarCabo()
        {
            return new Cabo
            {
                Tipo = Types.TipoCaboPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de cabo cadastrado.")
            };
        }
    }
}
