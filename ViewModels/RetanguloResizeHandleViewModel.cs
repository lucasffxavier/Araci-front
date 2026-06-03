namespace Araci.ViewModels
{
    public enum RetanguloResizeHandleKind
    {
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left
    }

    public class RetanguloResizeHandleViewModel
    {
        public RetanguloResizeHandleViewModel(RetanguloAnotativoViewModel retangulo, RetanguloResizeHandleKind kind, double x, double y, bool isActive)
        {
            Retangulo = retangulo;
            Kind = kind;
            X = x;
            Y = y;
            IsActive = isActive;
        }

        public RetanguloAnotativoViewModel Retangulo { get; }
        public RetanguloResizeHandleKind Kind { get; }
        public double X { get; }
        public double Y { get; }
        public bool IsActive { get; }
    }
}