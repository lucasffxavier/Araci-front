using System.Windows;
using Araci.Services;

namespace Araci
{
    public static class AppServices
    {
        public static ToolService Tools { get; } =
            new ToolService();

        public static ViewportService? Viewport { get; set; }

        public static FrameworkElement? ViewportReference { get; set; }

        // ✅ AGORA EXISTE E FUNCIONA
        public static EditorState Editor { get; } =
            new EditorState();

        public static MoveHudService MoveHud { get; } =
            new MoveHudService();
    }
}