using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Araci.Core.Viewport
{
    public class Camera : INotifyPropertyChanged
    {
        public const double MinZoom = 0.1;
        public const double MaxZoom = 8.0;

        private double _zoom = 1.0;
        private Point _offset;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double Zoom
        {
            get => _zoom;
            set
            {
                double zoom = Math.Max(MinZoom, Math.Min(MaxZoom, value));

                if (Math.Abs(_zoom - zoom) < 0.0001)
                    return;

                _zoom = zoom;
                OnPropertyChanged();
            }
        }

        public Point Offset
        {
            get => _offset;
            set
            {
                if (_offset == value)
                    return;

                _offset = value;
                OnPropertyChanged();
            }
        }

        public void Pan(Vector delta)
        {
            Offset = new Point(Offset.X + delta.X, Offset.Y + delta.Y);
        }

        public void ZoomAt(Point screenPoint, double factor)
        {
            Point worldBefore = ScreenToWorld(screenPoint);

            Zoom = Zoom * factor;

            Offset = new Point(
                screenPoint.X - worldBefore.X * Zoom,
                screenPoint.Y - worldBefore.Y * Zoom);
        }

        public void SetZoomAt(Point screenPoint, double zoom)
        {
            Point worldBefore = ScreenToWorld(screenPoint);

            Zoom = zoom;

            Offset = new Point(
                screenPoint.X - worldBefore.X * Zoom,
                screenPoint.Y - worldBefore.Y * Zoom);
        }

        public void Fit(Rect worldBounds, Size viewportSize, double margin)
        {
            if (worldBounds.IsEmpty || viewportSize.Width <= 0 || viewportSize.Height <= 0)
            {
                Reset();
                return;
            }

            double availableWidth = Math.Max(1, viewportSize.Width - margin * 2);
            double availableHeight = Math.Max(1, viewportSize.Height - margin * 2);
            double boundsWidth = Math.Max(1, worldBounds.Width);
            double boundsHeight = Math.Max(1, worldBounds.Height);

            Zoom = Math.Min(availableWidth / boundsWidth, availableHeight / boundsHeight);

            Point worldCenter = new(
                worldBounds.X + worldBounds.Width / 2,
                worldBounds.Y + worldBounds.Height / 2);

            Point screenCenter = new(
                viewportSize.Width / 2,
                viewportSize.Height / 2);

            Offset = new Point(
                screenCenter.X - worldCenter.X * Zoom,
                screenCenter.Y - worldCenter.Y * Zoom);
        }

        public void Reset()
        {
            Zoom = 1.0;
            Offset = new Point(0, 0);
        }

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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
