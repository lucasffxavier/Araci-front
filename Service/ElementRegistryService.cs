using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Rendering;
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

        public ElementRegistryService(TypeLibraryService types)
        {
            Types = types ?? throw new ArgumentNullException(nameof(types));

            RegistrarElementosPadrao();
        }

        private TypeLibraryService Types { get; }

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
            ElementDefinition? definition = FindByKind(kind);
            return definition == null ? null : CreateModel(definition);
        }

        public T CreateModel<T>() where T : Elemento
        {
            ElementDefinition? definition =
                _definitions.FirstOrDefault(d => d.ModelType == typeof(T));

            if (definition == null)
                throw new InvalidOperationException($"Elemento nao registrado: {typeof(T).Name}.");

            return (T)CreateModel(definition);
        }

        public ElementoViewModel? CreateViewModel(
            Elemento modelo,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs,
            TerminalLayoutService terminalLayout)
        {
            return FindByModel(modelo)?.CriarViewModel(
                modelo,
                names,
                typePropertiesDialogs,
                terminalLayout);
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

        private static Elemento CreateModel(ElementDefinition definition)
        {
            Elemento elemento = definition.CriarModelo();
            definition.AtualizarTerminais(elemento);
            return elemento;
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
                () => Types.TiposBarras,
                e => new Size(ElementGeometryDefaults.BarraLargura, ((Barra)e).Altura),
                e => ((Barra)e).AtualizarTerminais()));

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
                () => Types.TiposCargas,
                _ => EquipamentoSize(),
                e => ((Carga)e).AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura)));

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
                () => Types.TiposGeradores,
                _ => EquipamentoSize(),
                e => ((Gerador)e).AtualizarTerminais(
                    ElementGeometryDefaults.EquipamentoLargura,
                    ElementGeometryDefaults.EquipamentoAltura)));

            Register(new ElementDefinition(
                "Sin",
                "SIN",
                "SIN",
                typeof(Sin),
                typeof(SinViewModel),
                typeof(TipoSin),
                CriarSin,
                (m, n, d, l) => new SinViewModel((Sin)m, Types, n, d, l),
                () => Types.TipoSinPadrao,
                () => Types.TiposSin,
                _ => EquipamentoSize(),
                e => ((Sin)e).AtualizarTerminais(
                    ElementGeometryDefaults.EquipamentoLargura,
                    ElementGeometryDefaults.EquipamentoAltura)));

            Register(new ElementDefinition(
                "Transformador",
                "Transformador",
                "TR",
                typeof(Transformador),
                typeof(TransformadorViewModel),
                typeof(TipoTransformador),
                CriarTransformador,
                (m, n, d, l) => new TransformadorViewModel((Transformador)m, Types, n, d, l),
                () => Types.TipoTransformadorPadrao,
                () => Types.TiposTransformadores,
                _ => TransformadorSize(),
                e => ((Transformador)e).AtualizarTerminais(
                    ElementGeometryDefaults.TransformadorLargura,
                    ElementGeometryDefaults.TransformadorAltura)));

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
                () => Types.TiposCabos,
                _ => Size.Empty,
                e => AtualizarTerminaisCabo((Cabo)e)));
        }

        private Barra CriarBarra()
        {
            var barra = new Barra
            {
                Tipo = Types.TipoBarraPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de barra cadastrado.")
            };

            return barra;
        }

        private Carga CriarCarga()
        {
            var carga = new Carga
            {
                Tipo = Types.TipoCargaPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de carga cadastrado.")
            };

            return carga;
        }

        private Gerador CriarGerador()
        {
            var gerador = new Gerador
            {
                Tipo = Types.TipoGeradorPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de gerador cadastrado.")
            };

            return gerador;
        }

        private Sin CriarSin()
        {
            var sin = new Sin
            {
                Tipo = Types.TipoSinPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de SIN cadastrado.")
            };

            return sin;
        }

        private Transformador CriarTransformador()
        {
            var transformador = new Transformador
            {
                Tipo = Types.TipoTransformadorPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de transformador cadastrado.")
            };

            return transformador;
        }

        private Cabo CriarCabo()
        {
            return new Cabo
            {
                Tipo = Types.TipoCaboPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de cabo cadastrado.")
            };
        }

        private static Size EquipamentoSize()
        {
            return new Size(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);
        }

        private static Size TransformadorSize()
        {
            return new Size(
                ElementGeometryDefaults.TransformadorLargura,
                ElementGeometryDefaults.TransformadorAltura);
        }

        private static Size GetFallbackSize(Elemento elemento)
        {
            return elemento switch
            {
                Barra barra => new Size(ElementGeometryDefaults.BarraLargura, barra.Altura),
                ElementoEquipamento => EquipamentoSize(),
                ElementoLinear => Size.Empty,
                _ => EquipamentoSize()
            };
        }

        private static void AtualizarTerminaisCabo(Cabo cabo)
        {
            if (cabo.Vertices.Count > 0)
                cabo.DefinirOrigem(cabo.Vertices[0]);

            if (cabo.Vertices.Count > 1)
                cabo.DefinirDestino(cabo.Vertices[^1]);
        }
    }
}
