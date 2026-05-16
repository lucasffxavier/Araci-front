using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Transactions;
using Araci.Services;
using Araci.ViewModels;

namespace Araci
{
    [System.Obsolete(
        "AppServices será removido. Injete EditorContext diretamente.",
        error: false)]
    public static class AppServices
    {

        public static EditorContext Current
        { get; set; }
            = new EditorContext();

        public static void ResetContext(
            EditorContext? context = null)
        {
            Current =
                context ?? new EditorContext();
        }

        public static AraciDocument Document =>
            Current.Document;

        public static ToolService Tools =>
            Current.Tools;

        public static InputRouter Input =>
            Current.Input;

        public static ViewportService? Viewport =>
            Current.Viewport;

        public static EditorState Editor =>
            Current.Editor;

        public static MoveHudService MoveHud =>
            Current.MoveHud;

        public static SelectionBoxViewModel SelectionBox =>
            Current.SelectionBox;

        public static CommandManager Commands =>
            Current.Commands;

        public static SelectionService Selection =>
            Current.Selection;

        public static MoveService Move =>
            Current.Move;

        public static SnapService Snap =>
            Current.Snap;

        public static TypeLibraryService Types =>
            Current.Types;

        public static ElementoFactory ElementoFactory =>
            Current.ElementoFactory;

        public static TransactionScope BeginTransaction() =>
            Current.BeginTransaction();
    }
}