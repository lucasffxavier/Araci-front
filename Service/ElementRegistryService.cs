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
        public const string KindBarra = "Barra";
        public const string KindCarga = "Carga";
        public const string KindGerador = "Gerador";
        public const string KindSin = "Sin";
        public const string KindTransformador = "Transformador";
        public const string KindCabo = "Cabo";

        private readonly Dictionary<string, ElementDefinition> _porKind = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<ElementDefinition> _definitions = new();

        public ElementRegistryService(TypeLibraryService types)
        {
            Types = types ?? throw new ArgumentNullException(nameof(types));
            RegistrarElementosPadrao();
        }

        private TypeLibraryService Types { get; }
        public IReadOnlyList<ElementDefinition> Definitions => _definitions;
        public IEnumerable<ElementDefinition> RibbonDefinitions => _definitions.Where(d => d.ExibirNoRibbon).OrderBy(d => d.OrdemRibbon).ThenBy(d => d.NomeRibbon);

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
                : _porKind.TryGetValue(kind.Trim(), out ElementDefinition? definition)
                    ? definition
                    : null;
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

        public Elemento CreateModel(string kind)
        {
            ElementDefinition definition = FindByKind(kind) ?? throw new InvalidOperationException($"Elemento nao registrado: {kind}.");
            return CreateModel(definition);
        }

        public T CreateModel<T>() where T : Elemento
        {
            ElementDefinition definition = FindByModelType<T>() ?? throw new InvalidOperationException($"Elemento nao registrado: {typeof(T).Name}.");
            return (T)CreateModel(definition);
        }

        public T CreateModel<T>(string kind) where T : Elemento
        {
            Elemento modelo = CreateModel(kind);

            if (modelo is not T typed)
                throw new InvalidOperationException($"O elemento '{kind}' nao cria modelo do tipo {typeof(T).Name}.");

            return typed;
        }

        public ElementoViewModel? CreateViewModel(Elemento modelo, NameService names, TypePropertiesDialogService typePropertiesDialogs, TerminalLayoutService terminalLayout)
        {
            return FindByModel(modelo)?.CriarViewModel(modelo, names, typePropertiesDialogs, terminalLayout);
        }

        public TViewModel CreateViewModel<TViewModel>(string kind, NameService names, TypePropertiesDialogService typePropertiesDialogs, TerminalLayoutService terminalLayout)
            where TViewModel : ElementoViewModel
        {
            Elemento modelo = CreateModel(kind);
            ElementoViewModel? viewModel = CreateViewModel(modelo, names, typePropertiesDialogs, terminalLayout);

            if (viewModel is not TViewModel typed)
                throw new InvalidOperationException($"O elemento '{kind}' nao cria ViewModel do tipo {typeof(TViewModel).Name}.");

            return typed;
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

        private static Elemento CreateModel(ElementDefinition definition)
        {
            Elemento elemento = definition.CriarModelo();
            definition.AtualizarTerminais(elemento);
            return elemento;
        }

        private void RegistrarElementosPadrao()
        {
            Register(new ElementDefinition(
                KindCabo,
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
                e => AtualizarTerminaisCabo((Cabo)e),
                "Cabo",
                "Inserir",
                "cabo.png",
                10,
                true,
                "CB",
                true,
                new[]
                {
                    Prop<CaboViewModel>("Nome", "Nome", 10),
                    Prop<CaboViewModel>("BarraOrigem", "Barra origem", 20),
                    Prop<CaboViewModel>("BarraDestino", "Barra destino", 30),
                    Prop<CaboViewModel>("Comprimento", "Comprimento (m)", 40),
                    Prop<CaboViewModel>("Ampacidade", "Ampacidade (A)", 50),
                    Prop<CaboViewModel>("TensaoLinha", "Tensão linha (kV)", 60, allowMixedTypeEdit: true),
                    Prop<CaboViewModel>("TensaoFaseA", "Tensão fase A (kV)", 70, allowMixedTypeEdit: true),
                    Prop<CaboViewModel>("TensaoFaseB", "Tensão fase B (kV)", 80, allowMixedTypeEdit: true),
                    Prop<CaboViewModel>("TensaoFaseC", "Tensão fase C (kV)", 90, allowMixedTypeEdit: true),
                    Prop<CaboViewModel>("CorrenteLinha", "Corrente linha (A)", 100, allowMixedTypeEdit: true),
                    Prop<CaboViewModel>("CorrenteFaseA", "Corrente fase A (A)", 110, allowMixedTypeEdit: true),
                    Prop<CaboViewModel>("CorrenteFaseB", "Corrente fase B (A)", 120, allowMixedTypeEdit: true),
                    Prop<CaboViewModel>("CorrenteFaseC", "Corrente fase C (A)", 130, allowMixedTypeEdit: true)
                }));

            Register(new ElementDefinition(
                KindCarga,
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
                e => ((Carga)e).AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                "Carga",
                "Inserir",
                "carga.png",
                20,
                true,
                "CG",
                false,
                new[]
                {
                    Prop<CargaViewModel>("Nome", "Nome", 10),
                    Prop<CargaViewModel>("PotenciaAtiva", "Potência ativa (kW)", 20, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("PotenciaReativa", "Potência reativa (kVAr)", 30, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("Alimentador", "Alimentador", 40, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("CorrenteLinha", "Corrente linha (A)", 50, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("CorrenteFaseA", "Corrente fase A (A)", 60, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("CorrenteFaseB", "Corrente fase B (A)", 70, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("CorrenteFaseC", "Corrente fase C (A)", 80, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("TensaoLinha", "Tensão linha (kV)", 90, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("TensaoFaseA", "Tensão fase A (kV)", 100, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("TensaoFaseB", "Tensão fase B (kV)", 110, allowMixedTypeEdit: true),
                    Prop<CargaViewModel>("TensaoFaseC", "Tensão fase C (kV)", 120, allowMixedTypeEdit: true)
                }));

            Register(new ElementDefinition(
                KindGerador,
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
                e => ((Gerador)e).AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                "Gerador",
                "Inserir",
                "gerador.png",
                30,
                true,
                "GE",
                false,
                new[]
                {
                    Prop<GeradorViewModel>("Nome", "Nome", 10),
                    Prop<GeradorViewModel>("PotenciaAparente", "Potência aparente (kVA)", 20),
                    Prop<GeradorViewModel>("PotenciaAtiva", "Potência ativa (kW)", 30, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("PotenciaReativa", "Potência reativa (kVAr)", 40, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("Alimentador", "Alimentador", 45, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("TensaoLinha", "Tensão linha (kV)", 50, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("TensaoFaseA", "Tensão fase A (kV)", 60, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("TensaoFaseB", "Tensão fase B (kV)", 70, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("TensaoFaseC", "Tensão fase C (kV)", 80, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("CorrenteLinha", "Corrente linha (A)", 90, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("CorrenteFaseA", "Corrente fase A (A)", 100, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("CorrenteFaseB", "Corrente fase B (A)", 110, allowMixedTypeEdit: true),
                    Prop<GeradorViewModel>("CorrenteFaseC", "Corrente fase C (A)", 120, allowMixedTypeEdit: true)
                }));

            Register(new ElementDefinition(
                KindSin,
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
                e => ((Sin)e).AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                "SIN",
                "Inserir",
                "sin.png",
                40,
                true,
                "SI",
                false,
                new[]
                {
                    Prop<SinViewModel>("Nome", "Nome", 10),
                    Prop<SinViewModel>("TensaoLinha", "Tensão linha (kV)", 20, allowMixedTypeEdit: true),
                    Prop<SinViewModel>("Alimentador", "Alimentador", 30, allowMixedTypeEdit: true)
                }));

            Register(new ElementDefinition(
                KindTransformador,
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
                e => ((Transformador)e).AtualizarTerminais(ElementGeometryDefaults.TransformadorLargura, ElementGeometryDefaults.TransformadorAltura),
                "Trafo",
                "Inserir",
                "transformador.png",
                50,
                true,
                "TR",
                false,
                new[]
                {
                    Prop<TransformadorViewModel>("Nome", "Nome", 10),
                    Prop<TransformadorViewModel>("Barra", "Barra", 20),
                    Prop<TransformadorViewModel>("Alimentador", "Alimentador", 30, allowMixedTypeEdit: true),
                    Prop<TransformadorViewModel>("Fases", "Fases", 40),
                    Prop<TransformadorViewModel>("Enrolamentos", "Enrolamentos", 50),
                    Prop<TransformadorViewModel>("TensaoPrimarioKV", "Tensão primário (kV)", 60),
                    Prop<TransformadorViewModel>("TensaoSecundarioKV", "Tensão secundário (kV)", 70),
                    Prop<TransformadorViewModel>("PotenciaAparente", "Potência aparente (kVA)", 80),
                    Prop<TransformadorViewModel>("RPercentual", "R (%)", 90),
                    Prop<TransformadorViewModel>("XPercentual", "X (%)", 100),
                    Prop<TransformadorViewModel>("LigacaoPrimario", "Ligação primário", 110),
                    Prop<TransformadorViewModel>("LigacaoSecundario", "Ligação secundário", 120)
                }));

            Register(new ElementDefinition(
                KindBarra,
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
                e => ((Barra)e).AtualizarTerminais(ElementGeometryDefaults.BarraLargura),
                "Barra",
                "Inserir",
                "barra.png",
                60,
                true,
                "BA",
                false,
                new[]
                {
                    Prop<BarraViewModel>("Nome", "Nome", 10),
                    Prop<BarraViewModel>("Tensao", "Tensão (kV)", 20),
                    Prop<BarraViewModel>("Altura", "Altura (m)", 30)
                }));
        }

        private Barra CriarBarra()
        {
            return new Barra
            {
                Tipo = Types.TipoBarraPadrao ?? throw new InvalidOperationException("Nenhum tipo de barra cadastrado.")
            };
        }

        private Carga CriarCarga()
        {
            return new Carga
            {
                Tipo = Types.TipoCargaPadrao ?? throw new InvalidOperationException("Nenhum tipo de carga cadastrado.")
            };
        }

        private Gerador CriarGerador()
        {
            return new Gerador
            {
                Tipo = Types.TipoGeradorPadrao ?? throw new InvalidOperationException("Nenhum tipo de gerador cadastrado.")
            };
        }

        private Sin CriarSin()
        {
            return new Sin
            {
                Tipo = Types.TipoSinPadrao ?? throw new InvalidOperationException("Nenhum tipo de SIN cadastrado.")
            };
        }

        private Transformador CriarTransformador()
        {
            return new Transformador
            {
                Tipo = Types.TipoTransformadorPadrao ?? throw new InvalidOperationException("Nenhum tipo de transformador cadastrado.")
            };
        }

        private Cabo CriarCabo()
        {
            return new Cabo
            {
                Tipo = Types.TipoCaboPadrao ?? throw new InvalidOperationException("Nenhum tipo de cabo cadastrado.")
            };
        }

        private static Size EquipamentoSize()
        {
            return new Size(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura);
        }

        private static Size TransformadorSize()
        {
            return new Size(ElementGeometryDefaults.TransformadorLargura, ElementGeometryDefaults.TransformadorAltura);
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

        private static InstancePropertyDescriptor Prop<T>(string propertyName, string displayName, int order, bool isEditable = true, bool allowMixedTypeEdit = false)
            where T : ElementoViewModel
        {
            return new InstancePropertyDescriptor(typeof(T), propertyName, displayName, order, isEditable, allowMixedTypeEdit);
        }
    }
}