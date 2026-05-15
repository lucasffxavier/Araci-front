using System.Windows;

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

        public static EditorContext Context
        { get; private set; }
            = new EditorContext();

        public static void ResetContext(
            EditorContext? context = null)
        {
            Context =
                context ?? new EditorContext();
        }

        // =========================
        // DOCUMENTO CENTRAL
        // =========================

        public static AraciDocument Document
        {
            get => Context.Document;
            set => Context.Document = value;
        }

        // =========================
        // TOOLS
        // =========================

        public static ToolService Tools =>
            Context.Tools;

        // =========================
        // VIEWPORT
        // =========================

        public static ViewportService?
            Viewport
        {
            get => Context.Viewport;
            set => Context.Viewport = value;
        }

        public static FrameworkElement?
            ViewportReference
        {
            get => Context.ViewportReference;
            set => Context.ViewportReference = value;
        }

        // =========================
        // EDITOR
        // =========================

        public static EditorState Editor =>
            Context.Editor;

        // =========================
        // HUD
        // =========================

        public static MoveHudService MoveHud =>
            Context.MoveHud;

        // =========================
        // SELECTION BOX
        // =========================

        public static SelectionBoxViewModel SelectionBox =>
            Context.SelectionBox;

        // =========================
        // COMMANDS
        // =========================

        public static CommandManager Commands =>
            Context.Commands;

        // =========================
        // TRANSACTIONS
        // =========================

        public static TransactionScope BeginTransaction()
        {
            return Context.BeginTransaction();
        }

        // =========================
        // TYPES
        // =========================

        public static TypeLibraryService Types =>
            Context.Types;
    }
}
