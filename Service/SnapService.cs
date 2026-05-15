using System;
using System.Windows;

namespace Araci.Services
{
    public class SnapService
    {
        public bool Habilitado
        { get; set; }
            = true;

        public double GridSpacing
        { get; set; }
            = 20;

        public Point SnapPoint(Point point)
        {
            if (!Habilitado ||
                GridSpacing <= 0)
            {
                return point;
            }

            return new Point(
                SnapValue(point.X),
                SnapValue(point.Y));
        }

        public Vector SnapDelta(Vector delta)
        {
            Point snapped =
                SnapPoint(
                    new Point(
                        delta.X,
                        delta.Y));

            return new Vector(
                snapped.X,
                snapped.Y);
        }

        private double SnapValue(double value)
        {
            return Math.Round(value / GridSpacing) *
                   GridSpacing;
        }
    }
}
