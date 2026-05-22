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

            SceneQueries = new SceneQueryService(Scene);

            Snap = new SnapService(SceneQueries);

            ElementoFactory = new ElementoFactory(Types);

            Selection = new SelectionService(this);

            MoveHud = new MoveHudService(this);

            Move = new MoveService(this);

            Tools = new ToolService(this);

            Input = new InputRouter(this);
        }

        // =========================
        // EVENTOS
        // =========================

        public IEventBus Events { get; }

        // =========================
        // DOCUMENTO
        // =========================

        public AraciDocument Document { get; set; } = new AraciDocument();

        // =========================
        // SCENE
        // =========================

        public Scene Scene { get; } = new Scene();

        // =========================
        // SCENE QUERIES
        // =========================

        public ISceneQueryService SceneQueries { get; }

        // =========================
        // TOOLS
        // =========================

        public ToolService Tools { get; }

        // =========================
        // INPUT
        // =========================

        public InputRouter Input { get; }

        // =========================
        // VIEWPORT
        // =========================

        public ViewportService? Viewport { get; private set; }

        public void InicializarViewport(ViewportViewModel viewportViewModel)
        {
            Viewport = new ViewportService(viewportViewModel);
        }

        // =========================
        // EDITOR STATE
        // =========================

        public EditorState Editor { get; } = new EditorState();

        // =========================
        // HUD
        // =========================

        public MoveHudService MoveHud { get; }

        // =========================
        // SELECTION BOX
        // =========================

        public SelectionBoxViewModel SelectionBox { get; } = new SelectionBoxViewModel();

        // =========================
        // COMMANDS
        // =========================

        public CommandManager Commands { get; } = new CommandManager();

        // =========================
        // SELECTION
        // =========================

        public SelectionService Selection { get; }

        // =========================
        // MOVE
        // =========================

        public MoveService Move { get; }

        // =========================
        // SNAP
        // =========================

        public SnapService Snap { get; }

        // =========================
        // TYPES
        // =========================

        public TypeLibraryService Types { get; } = new TypeLibraryService();

        // =========================
        // FACTORY
        // =========================

        public ElementoFactory ElementoFactory { get; }

        // =========================
        // TRANSACTIONS
        // =========================

        public TransactionScope BeginTransaction()
        {
            return new TransactionScope(Commands);
        }
    }
}