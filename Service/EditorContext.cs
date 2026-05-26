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
            Snap = new SnapService(SceneQueries);
            Names = new NameService(Document);
            Connectivity = new ConnectivityService(this);
            Topology = new TopologyValidator(this);
            SimulationResults = new SimulationResultApplier(this);
            Simulation = new SimulationPipeline(this);
            SimulationExport = new SimulationExportService();
            SimulationMessages = new SimulationMessageBuilder();
            TypePropertiesDialogs = new TypePropertiesDialogService();
            Dialogs = new DialogService();

            ElementoFactory = new ElementoFactory(Types, Names, TypePropertiesDialogs);
            Selection = new SelectionService(this);
            MoveHud = new MoveHudService(this);
            MoveConstraints = new MoveConstraintService(Settings);
            Move = new MoveService(this);
            Tools = new ToolService(this);
            Input = new InputRouter(this);
            Navigation = new ViewportNavigationService(this);
        }

        public IEventBus Events { get; }

        public AraciDocument Document { get; set; } = new AraciDocument();

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

        public CommandManager Commands { get; } = new CommandManager();

        public SelectionService Selection { get; }

        public MoveService Move { get; }

        public SnapService Snap { get; }

        public NameService Names { get; }

        public ConnectivityService Connectivity { get; }

        public TopologyValidator Topology { get; }

        public SimulationResultApplier SimulationResults { get; }

        public SimulationPipeline Simulation { get; }

        public SimulationExportService SimulationExport { get; }

        public SimulationMessageBuilder SimulationMessages { get; }

        public TypePropertiesDialogService TypePropertiesDialogs { get; }

        public DialogService Dialogs { get; }

        public TypeLibraryService Types { get; } = new TypeLibraryService();

        public ElementoFactory ElementoFactory { get; }

        public TransactionScope BeginTransaction()
        {
            return new TransactionScope(Commands);
        }
    }
}
