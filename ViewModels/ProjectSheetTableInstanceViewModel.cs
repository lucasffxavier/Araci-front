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
        private double _sheetOriginOffsetX;
        private double _sheetOriginOffsetY;
        private bool _isSelected;

        public ProjectSheetTableInstanceViewModel(ProjectSheetTableInstance instance, string tableName)
        {
            ArgumentNullException.ThrowIfNull(instance);

            Id = instance.Id;
            TableId = instance.TableId;
            TableName = string.IsNullOrWhiteSpace(tableName) ? "Tabela sem nome" : tableName;
            _x = NormalizePosition(instance.X);
            _y = NormalizePosition(instance.Y);
            _width = NormalizeDimension(instance.Width, ProjectSheetTableInstance.MinWidth);
            _height = NormalizeDimension(instance.Height, ProjectSheetTableInstance.MinHeight);
        }

        public Guid Id { get; }
        public Guid TableId { get; }
        public string TableName { get; }
        public double ViewX => X + SheetOriginOffsetX;
        public double ViewY => Y + SheetOriginOffsetY;

        public double SheetOriginOffsetX
        {
            get => _sheetOriginOffsetX;
            private set
            {
                if (Math.Abs(_sheetOriginOffsetX - value) < 0.000001)
                    return;

                _sheetOriginOffsetX = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ViewX));
            }
        }

        public double SheetOriginOffsetY
        {
            get => _sheetOriginOffsetY;
            private set
            {
                if (Math.Abs(_sheetOriginOffsetY - value) < 0.000001)
                    return;

                _sheetOriginOffsetY = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ViewY));
            }
        }

        public double X
        {
            get => _x;
            private set
            {
                if (Math.Abs(_x - value) < 0.000001)
                    return;

                _x = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ViewX));
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
                OnPropertyChanged(nameof(ViewY));
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

        public void SetSheetOriginOffset(double offsetX, double offsetY)
        {
            SheetOriginOffsetX = NormalizeDimension(offsetX, 0);
            SheetOriginOffsetY = NormalizeDimension(offsetY, 0);
        }

        public void SetPreviewPosition(double x, double y)
        {
            X = NormalizePosition(x);
            Y = NormalizePosition(y);
        }

        public void SetPreviewSize(double width, double height)
        {
            Width = NormalizeDimension(width, ProjectSheetTableInstance.MinWidth);
            Height = NormalizeDimension(height, ProjectSheetTableInstance.MinHeight);
        }

        private static double NormalizePosition(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value)
                ? 0
                : value;
        }

        private static double NormalizeDimension(double value, double minimum)
        {
            double safeMinimum = double.IsNaN(minimum) || double.IsInfinity(minimum) || minimum < 0
                ? 0
                : minimum;

            return double.IsNaN(value) || double.IsInfinity(value) || value < safeMinimum
                ? safeMinimum
                : value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}