using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Araci.Core.Viewport
{
    public class Camera : INotifyPropertyChanged
    {
        private double _zoom = 1.0;
        private Point _offset;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double Zoom
        {
            get => _zoom;
            set
            {
                double zoom = Math.Max(double.Epsilon, value);

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