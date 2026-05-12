using System.Windows;

using Araci.Core.Documents;
using Araci.Services;
using Araci.Core.Commands;

namespace Araci
{
    public static class AppServices
    {
        // =========================
        // DOCUMENTO CENTRAL
        // =========================

        public static AraciDocument Document
        { get; set; }
            = new AraciDocument();

        // =========================
        // TOOLS
        // =========================

        public static ToolService Tools
        { get; }
            = new ToolService();

        // =========================
        // VIEWPORT
        // =========================

        public static ViewportService?
            Viewport
        { get; set; }

        public static FrameworkElement?
            ViewportReference
        { get; set; }

        // =========================
        // EDITOR
        // =========================

        public static EditorState Editor
        { get; }
            = new EditorState();

        // =========================
        // HUD
        // =========================

        public static MoveHudService MoveHud
        { get; }
            = new MoveHudService();

        // =========================
        // COMMANDS
        // =========================

        public static CommandManager
            Commands
        { get; }
            = new CommandManager();
    }
}