namespace Araci.Services
{
    public static class AppServices
    {
        public static ViewportService? Viewport { get; set; }

        public static EditorState Editor { get; }
            = new EditorState();

        public static ToolService Tools { get; }
            = new ToolService();

        // 🔥 NOVO
        public static MoveHudState MoveHud { get; }
            = new MoveHudState();
    }
}