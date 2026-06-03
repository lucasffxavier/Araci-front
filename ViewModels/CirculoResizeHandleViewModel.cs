namespace Araci.ViewModels
{
    public class CirculoResizeHandleViewModel
    {
        public CirculoResizeHandleViewModel(CirculoAnotativoViewModel circulo, double x, double y, bool isActive)
        {
            Circulo = circulo;
            X = x;
            Y = y;
            IsActive = isActive;
        }

        public CirculoAnotativoViewModel Circulo { get; }
        public double X { get; }
        public double Y { get; }
        public bool IsActive { get; }
    }
}