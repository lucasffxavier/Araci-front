using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Events;
using Araci.Core.SceneQueries;
using Araci.Core.Scenes;
using Araci.Core.Transactions;
using Araci.ViewModels;

namespace Araci.Services
{
    public class EditorContext
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
            SimulationResults = new SimulationResultApplier(this);
            Simulation = new SimulationPipeline(this);
            SimulationExport = new SimulationExportService();
            SimulationMessages = new SimulationMessageBuilder();
            TypePropertiesDialogs = new TypePropertiesDialogService();
            Dialogs = new DialogService();
            Elements = new ElementRegistryService(Types);
            Geometry = new ElementGeometryService(Elements);
            TerminalLayout = new TerminalLayoutService(Elements, Geometry);
            Names = new NameService(Document, Elements);

            ElementoFactory = new ElementoFactory(Elements, Names, TypePropertiesDialogs, TerminalLayout);
            CableVertexEdit = new CableVertexEditService(this);
            Selection = new SelectionService(this);
            Selection.SelectionChanged += CableVertexEdit.Refresh;
            SafeDelete = new SafeDeleteService(this);
            Projects = new ProjectPersistenceService(this);
            MoveHud = new MoveHudService(this);
            MoveConstraints = new MoveConstraintService(Settings);
            Move = new MoveService(this);
            Rotation = new RotationService(this);
            Tools = new ToolService(this);
            Input = new InputRouter(this);
            Navigation = new ViewportNavigationService(this);
        }

        public IEventBus Events { get; }

        public AraciDocument Document { get; } = new AraciDocument();

        public Scene Scene { get; } = new Scene();

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

        public MoveConstraintService MoveConstraints { get; }

        public SelectionBoxViewModel SelectionBox { get; } = new SelectionBoxViewModel();

        public TerminalSnapState TerminalSnap { get; } = new TerminalSnapState();

        public CableVertexEditService CableVertexEdit { get; }

        public CommandManager Commands { get; } = new CommandManager();

        public SafeDeleteService SafeDelete { get; }

        public ProjectPersistenceService Projects { get; }

        public SelectionService Selection { get; }

        public MoveService Move { get; }

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

        public ElementRegistryService Elements { get; }

        public TypeLibraryService Types { get; } = new TypeLibraryService();

        public ElementoFactory ElementoFactory { get; }

        public TransactionScope BeginTransaction()
        {
            return new TransactionScope(Commands);
        }
    }
}
