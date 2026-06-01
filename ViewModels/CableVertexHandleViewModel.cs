namespace Araci.ViewModels
{
    public class CableVertexHandleViewModel
    {
        public CableVertexHandleViewModel(CaboViewModel cabo, int indice, double x, double y, bool isActive)
        {
            Cabo = cabo;
            Indice = indice;
            X = x;
            Y = y;
            IsActive = isActive;
        }

        public CaboViewModel Cabo { get; }
        public int Indice { get; }
        public double X { get; }
        public double Y { get; }
        public bool IsActive { get; }
    }
}
