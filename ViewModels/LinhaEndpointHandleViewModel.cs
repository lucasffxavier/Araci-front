namespace Araci.ViewModels
{
    public enum LinhaEndpointKind
    {
        Inicio,
        Fim
    }

    public class LinhaEndpointHandleViewModel
    {
        public LinhaEndpointHandleViewModel(LinhaAnotativaViewModel linha, LinhaEndpointKind kind, double x, double y, bool isActive)
        {
            Linha = linha;
            Kind = kind;
            X = x;
            Y = y;
            IsActive = isActive;
        }

        public LinhaAnotativaViewModel Linha { get; }
        public LinhaEndpointKind Kind { get; }
        public double X { get; }
        public double Y { get; }
        public bool IsActive { get; }
    }
}
