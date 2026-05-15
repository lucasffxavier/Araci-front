using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Transactions;
using Araci.Services;
using Araci.ViewModels;

namespace Araci
{
    public static class AppServices
    {
        // =========================
        // CONTEXTO
        // =========================

        public static EditorContext Current
        { get; set; }
            = new EditorContext();

        public static void ResetContext(
            EditorContext? context = null)
        {
            Current =
                context ?? new EditorContext();
        }

        // =========================
        // DOCUMENTO CENTRAL
        // =========================

        public static AraciDocument Document =>
            Current.Document;

        // =========================
        // TOOLS
        // =========================

        public static ToolService Tools =>
            Current.Tools;

        // =========================
        // INPUT
        // =========================

        public static InputRouter Input =>
            Current.Input;

        // =========================
        // VIEWPORT
        // =========================

        public static ViewportService? Viewport =>
            Current.Viewport;

        // =========================
        // EDITOR
        // =========================

        public static EditorState Editor =>
            Current.Editor;

        // =========================
        // HUD
        // =========================

        public static MoveHudService MoveHud =>
            Current.MoveHud;

        // =========================
        // SELECTION BOX
        // =========================

        public static SelectionBoxViewModel SelectionBox =>
            Current.SelectionBox;

        // =========================
        // COMMANDS
        // =========================

        public static CommandManager Commands =>
            Current.Commands;

        // =========================
        // SELECTION
        // =========================

        public static SelectionService Selection =>
            Current.Selection;

        // =========================
        // MOVE
        // =========================

        public static MoveService Move =>
            Current.Move;

        // =========================
        // SNAP
        // =========================

        public static SnapService Snap =>
            Current.Snap;

        // =========================
        // TYPES
        // =========================

        public static TypeLibraryService Types =>
            Current.Types;

        // =========================
        // ELEMENTOS
        // =========================

        public static ElementoFactory ElementoFactory =>
            Current.ElementoFactory;

        // =========================
        // TRANSACTIONS
        // =========================

        public static TransactionScope BeginTransaction()
        {
            return Current.BeginTransaction();
        }
    }
}
