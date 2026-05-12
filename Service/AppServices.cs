using Araci.Services;

namespace Araci.Services
{
    public static class AppServices
    {
        // =========================
        // VIEWPORT
        // =========================

        public static ViewportService? Viewport
        {
            get;
            set;
        }

        // =========================
        // ESTADO GLOBAL
        // =========================

        public static EditorState Editor
        {
            get;
        }
        = new EditorState();

        // =========================
        // FERRAMENTAS
        // =========================

        public static ToolService Tools
        {
            get;
        }
        = new ToolService();
    }
}