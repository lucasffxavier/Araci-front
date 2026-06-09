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
        private bool _isSelected;

        public ProjectSheetTableInstanceViewModel(ProjectSheetTableInstance instance, string tableName)
        {
            ArgumentNullException.ThrowIfNull(instance);

            Id = instance.Id;
            TableId = instance.TableId;
            TableName = string.IsNullOrWhiteSpace(tableName) ? "Tabela sem nome" : tableName;
            _x = instance.X;
            _y = instance.Y;
            Width = instance.Width;
            Height = instance.Height;
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

        public double Width { get; }
        public double Height { get; }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
