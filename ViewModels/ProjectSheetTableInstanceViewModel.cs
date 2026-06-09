using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTableInstanceViewModel : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private bool _isSelected;

        public ProjectSheetTableInstanceViewModel(ProjectSheetTableInstance instance, string tableName)
        {
            ArgumentNullException.ThrowIfNull(instance);

            Id = instance.Id;
            TableId = instance.TableId;
            TableName = string.IsNullOrWhiteSpace(tableName) ? "Tabela sem nome" : tableName;
            _x = instance.X;
            _y = instance.Y;
            _width = instance.Width;
            _height = instance.Height;
        }

        public Guid Id { get; }
        public Guid TableId { get; }
        public string TableName { get; }

        public double X
        {
            get => _x;
            private set
            {
                if (Math.Abs(_x - value) < 0.000001)
                    return;

                _x = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _y;
            private set
            {
                if (Math.Abs(_y - value) < 0.000001)
                    return;

                _y = value;
                OnPropertyChanged();
            }
        }

        public double Width
        {
            get => _width;
            private set
            {
                if (Math.Abs(_width - value) < 0.000001)
                    return;

                _width = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get => _height;
            private set
            {
                if (Math.Abs(_height - value) < 0.000001)
                    return;

                _height = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public void SetPreviewPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void SetPreviewSize(double width, double height)
        {
            Width = NormalizeDimension(width, ProjectSheetTableInstance.MinWidth);
            Height = NormalizeDimension(height, ProjectSheetTableInstance.MinHeight);
        }

        private static double NormalizeDimension(double value, double minimum)
        {
            return double.IsNaN(value) || double.IsInfinity(value) || value < minimum
                ? minimum
                : value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}