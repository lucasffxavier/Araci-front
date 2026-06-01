using System.Windows;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public sealed class CableVertexSegmentHit
    {
        public CableVertexSegmentHit(CaboViewModel cabo, int insertIndex, Point point, double distanceSquared)
        {
            Cabo = cabo;
            InsertIndex = insertIndex;
            Point = point;
            DistanceSquared = distanceSquared;
        }

        public CaboViewModel Cabo { get; }
        public int InsertIndex { get; }
        public Point Point { get; }
        public double DistanceSquared { get; }
    }
}
