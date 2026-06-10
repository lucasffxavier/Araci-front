using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateLineViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetTemplateLine _linha;
        private bool _isSelected;
        private double _previewOffsetX;
        private double _previewOffsetY;

        public ProjectSheetTemplateLineViewModel(ProjectSheetTemplateLine linha)
        {
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
        }

        public Guid Id => _linha.Id;
        public double X1 => _linha.X1 + _previewOffsetX;
        public double Y1 => _linha.Y1 + _previewOffsetY;
        public double X2 => _linha.X2 + _previewOffsetX;
        public double Y2 => _linha.Y2 + _previewOffsetY;
        public double ModelX1 => _linha.X1;
        public double ModelY1 => _linha.Y1;
        public double ModelX2 => _linha.X2;
        public double ModelY2 => _linha.Y2;
        public Brush StrokeBrush => CriarBrush(_linha.Stroke);
        public double StrokeThickness => _linha.StrokeThickness;
        public bool Visible => _linha.Visible;
        public bool HasPreviewOffset => Math.Abs(_previewOffsetX) > 0.0001 || Math.Abs(_previewOffsetY) > 0.0001;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectionStrokeBrush));
                OnPropertyChanged(nameof(SelectionStrokeThickness));
            }
        }

        public Brush SelectionStrokeBrush => IsSelected ? Brushes.DodgerBlue : StrokeBrush;
        public double SelectionStrokeThickness => IsSelected ? Math.Max(StrokeThickness + 3.0, 4.0) : StrokeThickness;

        public void SetPreviewOffset(double deltaX, double deltaY)
        {
            if (Math.Abs(_previewOffsetX - deltaX) < 0.0001 && Math.Abs(_previewOffsetY - deltaY) < 0.0001)
                return;

            _previewOffsetX = deltaX;
            _previewOffsetY = deltaY;
            NotificarCoordenadas();
            OnPropertyChanged(nameof(HasPreviewOffset));
        }

        public void ClearPreviewOffset()
        {
            if (!HasPreviewOffset)
                return;

            _previewOffsetX = 0.0;
            _previewOffsetY = 0.0;
            NotificarCoordenadas();
            OnPropertyChanged(nameof(HasPreviewOffset));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotificarCoordenadas()
        {
            OnPropertyChanged(nameof(X1));
            OnPropertyChanged(nameof(Y1));
            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }

        private static Brush CriarBrush(string stroke)
        {
            try
            {
                if (ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(stroke) ? "#FF000000" : stroke) is Color color)
                    return new SolidColorBrush(color);
            }
            catch (FormatException)
            {
            }

            return Brushes.Black;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}