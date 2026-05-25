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
            Snap = new SnapService(SceneQueries);
            Connectivity = new ConnectivityService(this);
            Topology = new TopologyValidator(this);

            ElementoFactory = new ElementoFactory(Types);
            Selection = new SelectionService(this);
            MoveHud = new MoveHudService(this);
            Move = new MoveService(this);
            Tools = new ToolService(this);
            Input = new InputRouter(this);
            Navigation = new ViewportNavigationService(this);
        }

        public IEventBus Events { get; }

        public AraciDocument Document { get; set; } = new AraciDocument();

        public Scene Scene { get; } = new Scene();

        public ISceneQueryService SceneQueries { get; }

        public ToolService Tools { get; }

        public InputRouter Input { get; }

        public ViewportNavigationService Navigation { get; }

        public ViewportService? Viewport { get; private set; }

        public void InicializarViewport(ViewportViewModel viewportViewModel)
        {
            Viewport = new ViewportService(viewportViewModel);
        }

        public EditorState Editor { get; } = new EditorState();

        public MoveHudService MoveHud { get; }

        public SelectionBoxViewModel SelectionBox { get; } = new SelectionBoxViewModel();

        public CommandManager Commands { get; } = new CommandManager();

        public SelectionService Selection { get; }

        public MoveService Move { get; }

        public SnapService Snap { get; }

        public ConnectivityService Connectivity { get; }

        public TopologyValidator Topology { get; }

        public TypeLibraryService Types { get; } = new TypeLibraryService();

        public ElementoFactory ElementoFactory { get; }

        public TransactionScope BeginTransaction()
        {
            return new TransactionScope(Commands);
        }
    }
}
