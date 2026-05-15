using System.Windows;

using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Transactions;
using Araci.ViewModels;

namespace Araci.Services
{
    public class EditorContext
    {
        // =========================
        // DOCUMENTO
        // =========================

        public AraciDocument Document
        { get; set; }
            = new AraciDocument();

        // =========================
        // TOOLS
        // =========================

        public ToolService Tools
        { get; }
            = new ToolService();

        // =========================
        // VIEWPORT
        // =========================

        public ViewportService?
            Viewport
        { get; set; }

        public FrameworkElement?
            ViewportReference
        { get; set; }

        // =========================
        // EDITOR
        // =========================

        public EditorState Editor
        { get; }
            = new EditorState();

        // =========================
        // HUD
        // =========================

        public MoveHudService MoveHud
        { get; }
            = new MoveHudService();

        // =========================
        // SELECTION BOX
        // =========================

        public SelectionBoxViewModel SelectionBox
        { get; }
            = new SelectionBoxViewModel();

        // =========================
        // COMMANDS
        // =========================

        public CommandManager Commands
        { get; }
            = new CommandManager();

        // =========================
        // TYPES
        // =========================

        public TypeLibraryService Types
        { get; }
            = new TypeLibraryService();

        // =========================
        // TRANSACTIONS
        // =========================

        public TransactionScope BeginTransaction()
        {
            return new TransactionScope(Commands);
        }
    }
}
