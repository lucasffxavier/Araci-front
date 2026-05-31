using System.Collections.Generic;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Events;
using Araci.Core.SceneQueries;
using Araci.Core.Scenes;
using Araci.Core.Transactions;
using Araci.Infrastructure.Persistence;
using Araci.Applications.Simulation;
using Araci.Applications.UseCases.Editar;
using Araci.Applications.UseCases.Diagrama;
using Araci.Infrastructure.Simulation;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class EditorContext : IEditorSession
    {
        public EditorContext()
            : this(new EventBus())
        {
        }

        public EditorContext(IEventBus eventBus)
        {
            Events = eventBus;
            Scene = new Scene();
            SceneQueries = new SceneQueryService(Scene);
            Hover = new HoverService(SceneQueries);
            Snap = new SnapService(SceneQueries, Settings);
            Connectivity = new ConnectivityService(this);
            ElectricGraph = new ElectricGraphBuilder(this);
            OperationalState = new OperationalGraphStateBuilder();
            Topology = new TopologyValidator(this);
            SimulationResults = new SimulationResultApplier(Document, NotifySimulationResultViewModels);
            var simulationGateway = new FastApiOpenDssGateway();
            var circuitDtoBuilder = new CircuitDtoBuilder(Document);
            Simulation = new SimulationPipeline(circuitDtoBuilder, simulationGateway, SimulationResults);
            SimulationExport = new SimulationExportService();
            SimulationMessages = new SimulationMessageBuilder();
            TypePropertiesDialogs = new TypePropertiesDialogService();
            Dialogs = new DialogService();
            Elements = new ElementRegistryService(Types);
            InstancePropertyCatalog.Configure(Elements);
            Geometry = new ElementGeometryService(Elements);
            TerminalLayout = new TerminalLayoutService(Elements, Geometry);
            Names = new NameService(Document, Elements);
            GeometryUpdates = new ElementGeometryUpdateService(this);
            ElementoFactory = new ElementoFactory(Elements, Names, TypePropertiesDialogs, TerminalLayout, GeometryUpdates);
            InserirElemento = new InserirElementoUseCase(ElementoFactory, TerminalLayout, Commands, Document, Names);
            InserirCabo = new InserirCaboUseCase(ElementoFactory, Commands, Document, Names, cabo => Viewport?.ObterViewModel(cabo) as CaboViewModel);
            CopiarElementos = new CopiarElementosUseCase();
            ColarElementos = new ColarElementosUseCase(CopiarElementos, Document, Names, Commands, ObterDestinoColagem);
            CableVertexEdit = new CableVertexEditService(this);
            Selection = new SelectionService(this);
            Selection.SelectionChanged += CableVertexEdit.Refresh;
            SafeDelete = new SafeDeleteService(this);
            var projectSerializer = new ProjectSerializer(Elements, TerminalLayout, Geometry);
            var projectRepository = new FileSystemProjectRepository();
            var projectFileDialogs = new ProjectFileDialogService();
            Projects = new ProjectPersistenceService(
                this,
                projectSerializer,
                projectRepository,
                projectFileDialogs,
                Dialogs);
            MoveHud = new MoveHudService(this);
            AlignmentGuides = new AlignmentGuideService(this);
            MoveConstraints = new MoveConstraintService(Settings);
            Move = new MoveService(this);
            BarraResize = new BarraResizeService(this);
            Rotation = new RotationService(this);
            Tools = new ToolService(this);
            Input = new InputRouter(this);
            Navigation = new ViewportNavigationService(this);
        }

        public IEventBus Events { get; }
        public AraciDocument Document { get; } = new AraciDocument();
        public Scene Scene { get; }
        public ISceneQueryService SceneQueries { get; }
        public HoverService Hover { get; }
        public ToolService Tools { get; }
        public InputRouter Input { get; }
        public ViewportNavigationService Navigation { get; }
        public ViewportService? Viewport { get; private set; }

        public void InicializarViewport(ViewportViewModel viewportViewModel)
        {
            Viewport = new ViewportService(viewportViewModel);
        }

        public EditorState Editor { get; } = new EditorState();
        public EditorSettings Settings { get; } = new EditorSettings();
        public MoveHudService MoveHud { get; }
        public AlignmentGuideService AlignmentGuides { get; }
        public MoveConstraintService MoveConstraints { get; }
        public SelectionBoxViewModel SelectionBox { get; } = new SelectionBoxViewModel();
        public TerminalSnapState TerminalSnap { get; } = new TerminalSnapState();
        public CableVertexEditService CableVertexEdit { get; }
        public CommandManager Commands { get; } = new CommandManager();
        ICommandHistory IEditorSession.Commands => Commands;
        public SafeDeleteService SafeDelete { get; }
        public ProjectPersistenceService Projects { get; }
        public SelectionService Selection { get; }
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
                {
                    vm.NotificarPropriedades(
                        "CorrenteLinha",
                        "CorrenteFaseA",
                        "CorrenteFaseB",
                        "CorrenteFaseC");
                }
            }
        }

        private Point ObterDestinoColagem(IReadOnlyList<Elemento> copiados)
        {
            if (Input.PossuiUltimaPosicaoMouseMundo)
                return Input.UltimaPosicaoMouseMundo;

            if (Viewport != null)
                return Viewport.ScreenToWorld(Viewport.CentroTela);

            Point centro = ColarElementosUseCase.CalcularCentro(copiados);
            return new Point(
                centro.X + ColarElementosUseCase.OffsetPadrao,
                centro.Y + ColarElementosUseCase.OffsetPadrao);
        }
    }
}
