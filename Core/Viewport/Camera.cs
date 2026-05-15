using System;
using System.Windows;

namespace Araci.Core.Viewport
{
    public class Camera
    {
        private double _zoom = 1.0;

        public double Zoom
        {
            get => _zoom;
            set => _zoom = Math.Max(double.Epsilon, value);
        }

        public Point Offset
        { get; set; }

        public Point WorldToScreen(Point point)
        {
            return new Point(
                point.X * Zoom + Offset.X,
                point.Y * Zoom + Offset.Y);
        }

        public Point ScreenToWorld(Point point)
        {
            return new Point(
                (point.X - Offset.X) / Zoom,
                (point.Y - Offset.Y) / Zoom);
        }
    }
}
