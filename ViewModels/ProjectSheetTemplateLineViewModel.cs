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

        public ProjectSheetTemplateLineViewModel(ProjectSheetTemplateLine linha)
        {
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
        }

        public Guid Id => _linha.Id;
        public double X1 => _linha.X1;
        public double Y1 => _linha.Y1;
        public double X2 => _linha.X2;
        public double Y2 => _linha.Y2;
        public Brush StrokeBrush => CriarBrush(_linha.Stroke);
        public double StrokeThickness => _linha.StrokeThickness;
        public bool Visible => _linha.Visible;

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

        public event PropertyChangedEventHandler? PropertyChanged;

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