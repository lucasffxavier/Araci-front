using System.Windows;
using System.Windows.Input;

namespace Araci.Applications.Editar.Base
{
    public readonly struct ToolInputState
    {
        public ToolInputState(bool isControlPressed)
            : this(
                isControlPressed ? ModifierKeys.Control : ModifierKeys.None,
                null,
                0,
                default,
                default)
        {
        }

        public ToolInputState(
            ModifierKeys modifiers,
            MouseButton? button = null,
            int clickCount = 0,
            Point worldPosition = default,
            Point screenPosition = default)
        {
            Modifiers = modifiers;
            Button = button;
            ClickCount = clickCount;
            WorldPosition = worldPosition;
            ScreenPosition = screenPosition;
        }

        public ModifierKeys Modifiers { get; }

        public MouseButton? Button { get; }

        public int ClickCount { get; }

        public Point WorldPosition { get; }

        public Point ScreenPosition { get; }

        public bool IsControlPressed => Modifiers.HasFlag(ModifierKeys.Control);

        public bool IsShiftPressed => Modifiers.HasFlag(ModifierKeys.Shift);

        public bool IsAltPressed => Modifiers.HasFlag(ModifierKeys.Alt);

        public bool IsDoubleClick => ClickCount >= 2;

        public bool IsLeftButton => Button == MouseButton.Left;

        public bool IsMiddleButton => Button == MouseButton.Middle;

        public bool IsRightButton => Button == MouseButton.Right;

        public static ToolInputState Empty => new(ModifierKeys.None);
    }
}
