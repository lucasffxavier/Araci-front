using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Applications.Editor;
using Araci.Applications.Factories;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Events;
using Araci.Core.SceneQueries;
using Araci.Core.Transactions;
using Araci.Applications.Anotar.InserirLinha;
using Araci.Applications.Anotar.InserirRetangulo;
using Araci.Applications.Anotar.InserirCirculo;
using Araci.Applications.Anotar.InserirTexto;
using Araci.Applications.Diagrama.InserirCabo;
using Araci.Applications.Diagrama.InserirElemento;
using Araci.Applications.Editar.Alinhar;
using Araci.Applications.Editar.Deletar;
using Araci.Applications.Editar.Mover;
using Araci.Applications.Editar.Selecionar;
using Araci.Applications.Projects;
using Araci.Applications.Projects.Tables;
using Araci.Applications.Simulation;
using Araci.Applications.UseCases.Analise;
using Araci.Applications.UseCases.Editar;
using Araci.Applications.UseCases.Diagrama;
using Araci.Applications.UseCases.Projeto;
using Araci.Models;
using Araci.Services.Composition;
using Araci.ViewModels;
using CoreScene = Araci.Core.Scenes.Scene;
using Araci.Services.Geometry;
using Araci.Services.Topology;
using Araci.Services.Editing;
using Araci.Services.Viewport;
using Araci.Services.Simulation;
using Araci.Services.UI;
using Araci.Services.Settings;
using Araci.Services.Catalog;
using Araci.Services.Naming;
using Araci.Services.Interaction;

namespace Araci.Services
{
    public class EditorContext : IEditorSession
    {
        private bool _aplicandoCameraVista;

        public EditorContext()
            : this(new EventBus())
        {
        }

        public EditorContext(IEventBus eventBus)
        {
            Events = eventBus;
            Document.VistaAtivaAlterada += OnDocumentoVistaAtivaAlterada;

            var core = EditorCoreComposition.Create(Document, Settings, Types);
            Scene = core.Scene;
            SceneQueries = core.SceneQueries;
            Hover = core.Hover;
            Snap = core.Snap;
            TypePropertiesDialogs = core.TypePropertiesDialogs;
            Dialogs = core.Dialogs;
            Elements = core.Elements;
            Connectivity = core.Connectivity;
            ElectricGraph = core.ElectricGraph;
            OperationalState = core.OperationalState;
            Topology = core.Topology;
            Geometry = core.Geometry;
            TerminalLayout = core.TerminalLayout;
            TypePropertiesDialogs.Configure(Commands, NotifyTextAnnotationTypeLibraryChanged, () => Document.Elementos.OfType<TextoAnotativo>());

            var simulation = SimulationComposition.Create(Document, NotifySimulationResultViewModels, Dialogs);
            SimulationResults = simulation.Results;
            Simulation = simulation.Pipeline;
            SimulationExport = simulation.Export;
            SimulationMessages = simulation.Messages;
            ExecutarSimulacao = simulation.ExecutarSimulacao;

            VisualUpdates = EditingComposition.CreateVisualUpdates(() => Viewport, TerminalLayout, Connectivity, SceneQueries, TerminalSnap, RefreshEditingHandles);
            Names = new NameService(Document, Elements);
            GeometryUpdates = new ElementGeometryUpdateService(TerminalLayout, Connectivity, VisualUpdates);
            var elementoModelFactory = new ElementoModelFactory(Elements);
            var elementoViewModelFactory = new ElementoViewModelFactory(Elements, elementoModelFactory, Names, TypePropertiesDialogs, TerminalLayout, GeometryUpdates);
            ElementoFactory = new ElementoFactory(elementoModelFactory, elementoViewModelFactory);
            InserirElemento = new InserirElementoUseCase(ElementoFactory, TerminalLayout, Commands, Document, Names);
            InserirCabo = new InserirCaboUseCase(ElementoFactory, Commands, Document, Names, cabo => Viewport?.ObterViewModel(cabo) as CaboViewModel);
            CopiarElementos = new CopiarElementosUseCase();
            ColarElementos = new ColarElementosUseCase(CopiarElementos, Document, Names, Commands, ObterDestinoColagem);
            ExcluirElemento = new ExcluirElementoUseCase(Document, Connectivity, Commands);
            EditarPropriedades = new EditarPropriedadesUseCase(Commands);
            Selection = EditingComposition.CreateSelection(Editor, Events, EditarPropriedades, Settings);
            SelecionarElementos = new SelecionarElementosUseCase(Selection);
            AtualizarPropriedadesSelecionadas = new AtualizarPropriedadesSelecionadasUseCase(Selection);
            EditarVerticesCabo = new EditarVerticesCaboUseCase(Commands);
            CableVertexEdit = EditingComposition.CreateCableVertexEdit(Selection, SceneQueries, VisualUpdates, EditarVerticesCabo);
            Selection.SelectionChanged += CableVertexEdit.Refresh;
            SafeDelete = EditingComposition.CreateSafeDelete(Selection, CableVertexEdit, ExcluirElemento, Hover, TerminalSnap, SceneQueries);
            Clipboard = EditingComposition.CreateClipboard(CopiarElementos, ColarElementos, Selection, () => Viewport, SceneQueries, CableVertexEdit);
            Projects = PersistenceComposition.CreateProjects(Document, Commands, Elements, elementoModelFactory, TerminalLayout, Geometry, Dialogs, Settings, LimparEstadoTransitorioProjeto);
            NovoProjeto = new NovoProjetoUseCase(Projects);
            AbrirProjeto = new AbrirProjetoUseCase(Projects);
            SalvarProjeto = new SalvarProjetoUseCase(Projects);
            CriarItemProjeto = new CriarItemProjetoUseCase(Document, Commands);
            RenomearItemProjeto = new RenomearItemProjetoUseCase(Document, Commands);
            ExcluirItemProjeto = new ExcluirItemProjetoUseCase(Document, Commands);
            DuplicarItemProjeto = new DuplicarItemProjetoUseCase(Document, Commands);
            EditarPropriedadesVista = new EditarPropriedadesVistaUseCase(Document, Commands);
            EditarPropriedadesTabela = new EditarPropriedadesTabelaUseCase(Document, Commands);
            EditarPropriedadesPrancha = new EditarPropriedadesPranchaUseCase(Document, Commands);
            EditarPropriedadesTipoPrancha = new EditarPropriedadesTipoPranchaUseCase(Document, Commands);
            InserirLinhaNoTipoPrancha = new InserirLinhaNoTipoPranchaUseCase(Document, Commands);
            InserirRetanguloNoTipoPrancha = new InserirRetanguloNoTipoPranchaUseCase(Document, Commands);
            InserirCirculoNoTipoPrancha = new InserirCirculoNoTipoPranchaUseCase(Document, Commands);
            InserirTextoNoTipoPrancha = new InserirTextoNoTipoPranchaUseCase(Document, Commands);
            ExcluirTextoDoTipoPrancha = new ExcluirTextoDoTipoPranchaUseCase(Document, Commands);
            MoverTextoDoTipoPrancha = new MoverTextoDoTipoPranchaUseCase(Document, Commands);
            ExcluirLinhaDoTipoPrancha = new ExcluirLinhaDoTipoPranchaUseCase(Document, Commands);
            MoverLinhaDoTipoPrancha = new MoverLinhaDoTipoPranchaUseCase(Document, Commands);
            ExcluirRetanguloDoTipoPrancha = new ExcluirRetanguloDoTipoPranchaUseCase(Document, Commands);
            MoverRetanguloDoTipoPrancha = new MoverRetanguloDoTipoPranchaUseCase(Document, Commands);
            ExcluirCirculoDoTipoPrancha = new ExcluirCirculoDoTipoPranchaUseCase(Document, Commands);
            MoverCirculoDoTipoPrancha = new MoverCirculoDoTipoPranchaUseCase(Document, Commands);
            ExportarTabela = new ExportarTabelaUseCase(Document, Dialogs, new ProjectTableDataBuilder(), new ProjectTableCsvExportService());
            InserirTabelaNaPrancha = new InserirTabelaNaPranchaUseCase(Document, Commands);
            InserirVistaNaPrancha = new InserirVistaNaPranchaUseCase(Document, Commands);
            MoverTabelaNaPrancha = new MoverTabelaNaPranchaUseCase(Document, Commands);
            RedimensionarTabelaNaPrancha = new RedimensionarTabelaNaPranchaUseCase(Document, Commands);
            RemoverTabelaDaPrancha = new RemoverTabelaDaPranchaUseCase(Document, Commands);
            DividirTabelaNaPrancha = new DividirTabelaNaPranchaUseCase(Document, Commands);

            var moveServices = EditingComposition.CreateMoveServices(() => Viewport, () => Scene.Elementos, Settings, Connectivity, TerminalLayout, SceneQueries, VisualUpdates, Selection, GeometryUpdates, Commands);
            MoveHud = moveServices.MoveHud;
            AlignmentGuides = moveServices.AlignmentGuides;
            MoveConstraints = moveServices.MoveConstraints;
            MoverElemento = moveServices.MoverElemento;
            RotacionarElemento = moveServices.RotacionarElemento;
            RedimensionarBarra = moveServices.RedimensionarBarra;
            Move = moveServices.Move;
            BarraResize = moveServices.BarraResize;
            Rotation = moveServices.Rotation;
            LinhaEndpointEdit = EditingComposition.CreateLinhaEndpointEdit(Selection, SceneQueries, MoverElemento, VisualUpdates);
            Selection.SelectionChanged += LinhaEndpointEdit.Refresh;
            RetanguloResize = EditingComposition.CreateRetanguloResize(Selection, SceneQueries, MoverElemento, VisualUpdates);
            Selection.SelectionChanged += RetanguloResize.Refresh;
            CirculoResize = EditingComposition.CreateCirculoResize(Selection, SceneQueries, MoverElemento, VisualUpdates);
            Selection.SelectionChanged += CirculoResize.Refresh;

            Tools = new ToolService(
                Elements,
                CriarSelecionarTool,
                CriarMoverTool,
                CriarAlinharTool,
                () => new DeletarTool(SafeDelete),
                CriarInserirCaboTool,
                CriarInserirElementoGenericoTool,
                CriarInserirLinhaAnotativaTool,
                CriarInserirRetanguloAnotativoTool,
                CriarInserirCirculoAnotativoTool,
                CriarInserirLinhaTipoPranchaTool,
                CriarInserirRetanguloTipoPranchaTool,
                CriarInserirCirculoTipoPranchaTool,
                CriarInserirTextoTipoPranchaTool,
                () => ProjectSheetTypeViewModelAtivo != null || Editor.SuperficieAtiva == EditorSurfaceKind.ProjectSheetType);

            Input = EditingComposition.CreateInput(Tools, Commands, SafeDelete, Selection, Elements, Hover, Clipboard.CopiarSelecionados, Clipboard.Colar);
            AlterarUnidadesProjeto = new AlterarUnidadesProjetoUseCase(Settings, RefreshProperties);
            Navigation = ViewportComposition.CreateNavigation(() => Viewport);
        }

        public IEventBus Events { get; }
        public AraciDocument Document { get; } = new AraciDocument();
        public CoreScene Scene { get; }
        public ISceneQueryService SceneQueries { get; }
        public HoverService Hover { get; }
        public ToolService Tools { get; }
        public InputRouter Input { get; }
        public ViewportNavigationService Navigation { get; }
        public ViewportService? Viewport { get; private set; }

        public ViewportViewModel CriarViewportViewModel()
        {
            return ViewportComposition.CreateViewModel(Document, Scene, SelectionBox, TerminalSnap, CableVertexEdit, LinhaEndpointEdit, RetanguloResize, CirculoResize, MoveHud, AlignmentGuides, ElementoFactory, Selection, Hover, SceneQueries);
        }

        public void InicializarViewport(ViewportViewModel viewportViewModel)
        {
            if (Viewport != null)
                Viewport.Camera.PropertyChanged -= OnCameraChanged;

            Viewport = new ViewportService(viewportViewModel);
            Viewport.Camera.PropertyChanged += OnCameraChanged;
            AplicarCameraVistaAtiva();
        }

        public void DefinirVistaAtiva(Guid vistaId)
        {
            SalvarCameraVistaAtiva();
            Document.DefinirVistaAtiva(vistaId);
        }

        public void RefreshProperties()
        {
            AtualizarPropriedadesSelecionadas.Executar();
        }

        public void SelecionarRegiaoRecorteVistaAtivaNasPropriedades()
        {
            ProjectView? vista = Document.VistaAtiva;

            if (vista == null)
                return;

            Selection.Limpar();
            Hover.Clear();
            Editor.SuperficieAtiva = EditorSurfaceKind.Diagram;
            Editor.ElementoSelecionado = new ProjectViewCropRegionPropertiesViewModel(
                Document,
                vista,
                EditarPropriedadesVista);
        }

        public EditorState Editor { get; } = new EditorState();
        public ProjectSheetTypeViewModel? ProjectSheetTypeViewModelAtivo { get; set; }
        public EditorSettings Settings { get; } = new EditorSettings();
        public MoveHudService MoveHud { get; }
        public AlignmentGuideService AlignmentGuides { get; }
        public MoveConstraintService MoveConstraints { get; }
        public SelectionBoxViewModel SelectionBox { get; } = new SelectionBoxViewModel();
        public TerminalSnapState TerminalSnap { get; } = new TerminalSnapState();
        public CableVertexEditService CableVertexEdit { get; }
        public LinhaEndpointEditService LinhaEndpointEdit { get; }
        public RetanguloResizeService RetanguloResize { get; }
        public CirculoResizeService CirculoResize { get; }
        public CommandManager Commands { get; } = new CommandManager();
        ICommandHistory IEditorSession.Commands => Commands;
        public SafeDeleteService SafeDelete { get; }
        public ClipboardService Clipboard { get; }
        public ProjectPersistenceService Projects { get; }
        public NovoProjetoUseCase NovoProjeto { get; }
        public AbrirProjetoUseCase AbrirProjeto { get; }
        public SalvarProjetoUseCase SalvarProjeto { get; }
        public CriarItemProjetoUseCase CriarItemProjeto { get; }
        public RenomearItemProjetoUseCase RenomearItemProjeto { get; }
        public ExcluirItemProjetoUseCase ExcluirItemProjeto { get; }
        public DuplicarItemProjetoUseCase DuplicarItemProjeto { get; }
        public EditarPropriedadesVistaUseCase EditarPropriedadesVista { get; }
        public EditarPropriedadesTabelaUseCase EditarPropriedadesTabela { get; }
        public EditarPropriedadesPranchaUseCase EditarPropriedadesPrancha { get; }
        public EditarPropriedadesTipoPranchaUseCase EditarPropriedadesTipoPrancha { get; }
        public InserirLinhaNoTipoPranchaUseCase InserirLinhaNoTipoPrancha { get; }
        public InserirRetanguloNoTipoPranchaUseCase InserirRetanguloNoTipoPrancha { get; }
        public InserirCirculoNoTipoPranchaUseCase InserirCirculoNoTipoPrancha { get; }
        public InserirTextoNoTipoPranchaUseCase InserirTextoNoTipoPrancha { get; }
        public ExcluirTextoDoTipoPranchaUseCase ExcluirTextoDoTipoPrancha { get; }
        public MoverTextoDoTipoPranchaUseCase MoverTextoDoTipoPrancha { get; }
        public ExcluirLinhaDoTipoPranchaUseCase ExcluirLinhaDoTipoPrancha { get; }
        public MoverLinhaDoTipoPranchaUseCase MoverLinhaDoTipoPrancha { get; }
        public ExcluirRetanguloDoTipoPranchaUseCase ExcluirRetanguloDoTipoPrancha { get; }
        public MoverRetanguloDoTipoPranchaUseCase MoverRetanguloDoTipoPrancha { get; }
        public ExcluirCirculoDoTipoPranchaUseCase ExcluirCirculoDoTipoPrancha { get; }
        public MoverCirculoDoTipoPranchaUseCase MoverCirculoDoTipoPrancha { get; }
        public ExportarTabelaUseCase ExportarTabela { get; }
        public InserirTabelaNaPranchaUseCase InserirTabelaNaPrancha { get; }
        public InserirVistaNaPranchaUseCase InserirVistaNaPrancha { get; }
        public MoverTabelaNaPranchaUseCase MoverTabelaNaPrancha { get; }
        public RedimensionarTabelaNaPranchaUseCase RedimensionarTabelaNaPrancha { get; }
        public RemoverTabelaDaPranchaUseCase RemoverTabelaDaPrancha { get; }
        public DividirTabelaNaPranchaUseCase DividirTabelaNaPrancha { get; }
        public VisualUpdateService VisualUpdates { get; }
        public SelectionService Selection { get; }
        public SelecionarElementosUseCase SelecionarElementos { get; }
        public AtualizarPropriedadesSelecionadasUseCase AtualizarPropriedadesSelecionadas { get; }
        public MoveService Move { get; }
        public BarraResizeService BarraResize { get; }
        public RotationService Rotation { get; }
        public SnapService Snap { get; }
        public NameService Names { get; }
        public ConnectivityService Connectivity { get; }
        public ElectricGraphBuilder ElectricGraph { get; }
        public OperationalGraphStateBuilder OperationalState { get; }
        public TopologyValidator Topology { get; }
        public SimulationResultApplier SimulationResults { get; }
        public SimulationPipeline Simulation { get; }
        public SimulationExportService SimulationExport { get; }
        public SimulationMessageBuilder SimulationMessages { get; }
        public ExecutarSimulacaoUseCase ExecutarSimulacao { get; }
        public TypePropertiesDialogService TypePropertiesDialogs { get; }
        public DialogService Dialogs { get; }
        public ElementGeometryService Geometry { get; }
        public TerminalLayoutService TerminalLayout { get; }
        public ElementGeometryUpdateService GeometryUpdates { get; }
        public ElementRegistryService Elements { get; }
        public TypeLibraryService Types { get; } = new TypeLibraryService();
        public ElementoFactory ElementoFactory { get; }
        public InserirElementoUseCase InserirElemento { get; }
        public InserirCaboUseCase InserirCabo { get; }
        public CopiarElementosUseCase CopiarElementos { get; }
        public ColarElementosUseCase ColarElementos { get; }
        public ExcluirElementoUseCase ExcluirElemento { get; }
        public EditarPropriedadesUseCase EditarPropriedades { get; }
        public MoverElementoUseCase MoverElemento { get; }
        public RotacionarElementoUseCase RotacionarElemento { get; }
        public RedimensionarBarraUseCase RedimensionarBarra { get; }
        public EditarVerticesCaboUseCase EditarVerticesCabo { get; }
        public AlterarUnidadesProjetoUseCase AlterarUnidadesProjeto { get; }

        public TransactionScope BeginTransaction()
        {
            return Commands.BeginTransaction();
        }

        private void NotifySimulationResultViewModels()
        {
            if (Viewport == null)
                return;

            foreach (ElementoViewModel vm in Viewport.Elementos)
            {
                if (vm.Modelo is Cabo or Carga)
                    vm.NotificarPropriedades("CorrenteLinha", "CorrenteFaseA", "CorrenteFaseB", "CorrenteFaseC");
            }
        }

        private void OnDocumentoVistaAtivaAlterada()
        {
            AplicarCameraVistaAtiva();
        }

        private void OnCameraChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_aplicandoCameraVista)
                return;

            SalvarCameraVistaAtiva();
        }

        private void SalvarCameraVistaAtiva()
        {
            if (Viewport == null || Document.VistaAtiva == null)
                return;

            Document.VistaAtiva.CameraX = Viewport.Camera.Offset.X;
            Document.VistaAtiva.CameraY = Viewport.Camera.Offset.Y;
            Document.VistaAtiva.Zoom = Viewport.Camera.Zoom;
        }

        private void AplicarCameraVistaAtiva()
        {
            if (Viewport == null || Document.VistaAtiva == null)
                return;

            double cameraX = Document.VistaAtiva.CameraX;
            double cameraY = Document.VistaAtiva.CameraY;
            double zoom = Document.VistaAtiva.Zoom;

            _aplicandoCameraVista = true;

            try
            {
                Viewport.Camera.Offset = new Point(cameraX, cameraY);
                Viewport.Camera.Zoom = zoom;
            }
            finally
            {
                _aplicandoCameraVista = false;
            }
        }

        private void NotifyTextAnnotationTypeLibraryChanged()
        {
            if (Viewport == null)
                return;

            foreach (ElementoViewModel vm in Viewport.Elementos)
            {
                if (vm is TextoAnotativoViewModel texto)
                    texto.AtualizarAposTipoAlterado();
            }

            RefreshProperties();
            SceneQueries.Invalidate();
        }

        private SelecionarTool CriarSelecionarTool()
        {
            return new SelecionarTool(SceneQueries, Selection, SelectionBox, CableVertexEdit, LinhaEndpointEdit, RetanguloResize, CirculoResize, BarraResize, Move, MoveHud, AlignmentGuides, MoveConstraints, Rotation);
        }

        private MoverTool CriarMoverTool()
        {
            return new MoverTool(SceneQueries, Selection, SelectionBox, CableVertexEdit, LinhaEndpointEdit, RetanguloResize, CirculoResize, BarraResize, Move, MoveHud, AlignmentGuides, MoveConstraints, Rotation);
        }

        private AlinharTool CriarAlinharTool()
        {
            return new AlinharTool(Hover, AlignmentGuides, Commands, SceneQueries);
        }

        private InserirCaboTool CriarInserirCaboTool()
        {
            return new InserirCaboTool(Commands, ElementoFactory, InserirCabo, Snap, Connectivity, AlignmentGuides, Scene, SceneQueries, TerminalSnap, () => Tools.VoltarParaSelecao());
        }

        private InserirElementoGenericoTool CriarInserirElementoGenericoTool(ElementDefinition definition)
        {
            return new InserirElementoGenericoTool(definition, ElementoFactory, InserirElemento, Snap, Geometry, TerminalLayout, AlignmentGuides, Scene, SceneQueries, () => Tools.VoltarParaSelecao());
        }

        private InserirLinhaAnotativaTool CriarInserirLinhaAnotativaTool()
        {
            return new InserirLinhaAnotativaTool(Commands, Document, Names, ElementoFactory, Scene, SceneQueries, LinhaEndpointEdit, () => Tools.VoltarParaSelecao());
        }

        private InserirLinhaTipoPranchaTool CriarInserirLinhaTipoPranchaTool()
        {
            return new InserirLinhaTipoPranchaTool(() => ProjectSheetTypeViewModelAtivo, InserirLinhaNoTipoPrancha, () => Tools.VoltarParaSelecao());
        }

        private InserirRetanguloAnotativoTool CriarInserirRetanguloAnotativoTool()
        {
            return new InserirRetanguloAnotativoTool(Commands, Document, Names, ElementoFactory, Scene, SceneQueries, () => Tools.VoltarParaSelecao());
        }

        private InserirRetanguloTipoPranchaTool CriarInserirRetanguloTipoPranchaTool()
        {
            return new InserirRetanguloTipoPranchaTool(() => ProjectSheetTypeViewModelAtivo, InserirRetanguloNoTipoPrancha, () => Tools.VoltarParaSelecao());
        }

        private InserirCirculoTipoPranchaTool CriarInserirCirculoTipoPranchaTool()
        {
            return new InserirCirculoTipoPranchaTool(() => ProjectSheetTypeViewModelAtivo, InserirCirculoNoTipoPrancha, () => Tools.VoltarParaSelecao());
        }

        private InserirTextoTipoPranchaTool CriarInserirTextoTipoPranchaTool()
        {
            return new InserirTextoTipoPranchaTool(() => ProjectSheetTypeViewModelAtivo, InserirTextoNoTipoPrancha, () => Tools.VoltarParaSelecao());
        }

        private InserirCirculoAnotativoTool CriarInserirCirculoAnotativoTool()
        {
            return new InserirCirculoAnotativoTool(Commands, Document, Names, ElementoFactory, Scene, SceneQueries, () => Tools.VoltarParaSelecao());
        }

        private Point ObterDestinoColagem(IReadOnlyList<Elemento> copiados)
        {
            if (Input.PossuiUltimaPosicaoMouseMundo)
                return Input.UltimaPosicaoMouseMundo;

            if (Viewport != null)
                return Viewport.ScreenToWorld(Viewport.CentroTela);

            Point centro = ColarElementosUseCase.CalcularCentro(copiados);
            return new Point(centro.X + ColarElementosUseCase.OffsetPadrao, centro.Y + ColarElementosUseCase.OffsetPadrao);
        }

        private void RefreshEditingHandles()
        {
            CableVertexEdit?.Refresh();
            LinhaEndpointEdit?.Refresh();
            RetanguloResize?.Refresh();
            CirculoResize?.Refresh();
        }

        private void LimparEstadoTransitorioProjeto()
        {
            Selection.Limpar();
            Hover.Clear();
            CableVertexEdit.Clear();
            LinhaEndpointEdit.Clear();
            RetanguloResize.Clear();
            CirculoResize.Clear();
            TerminalSnap.Limpar();
            SelectionBox.Visivel = false;
            MoveHud.Visivel = false;
            MoveHud.Reset();
            SceneQueries.Invalidate();
            Tools.VoltarParaSelecao();
        }
    }
}