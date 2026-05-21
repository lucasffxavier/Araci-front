namespace Araci.Applications.Editar.Base
{
    public readonly struct ToolInputState
    {
        public ToolInputState(bool isControlPressed)
        {
            IsControlPressed = isControlPressed;
        }

        public bool IsControlPressed { get; }

        public static ToolInputState Empty => new(false);
    }
}