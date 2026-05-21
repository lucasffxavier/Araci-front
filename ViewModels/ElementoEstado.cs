using System.Collections.Generic;
using System.Windows;

namespace Araci.ViewModels
{
    public class ElementoEstado
    {
        public double X { get; }
        public double Y { get; }
        public double X2 { get; }
        public double Y2 { get; }
        public List<Point> Vertices { get; }

        public ElementoEstado(
            double x,
            double y,
            double x2 = 0,
            double y2 = 0,
            IEnumerable<Point>? vertices = null)
        {
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;

            Vertices = vertices != null
                ? new List<Point>(vertices)
                : new List<Point>();
        }
    }
}