using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Araci.Core.Documents;
using Araci.Models.Tipos;
using Araci.Services.Catalog;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateCircleViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetTemplateCircle _circulo;
        private readonly TypeLibraryService _types;
        private TipoLinhaAnotativa? _tipoLinha;
        private bool _isSelected;
        private double _previewOffsetX;
        private double _previewOffsetY;

        public ProjectSheetTemplateCircleViewModel(ProjectSheetTemplateCircle circulo)
            : this(circulo, new TypeLibraryService())
        {
        }

        public ProjectSheetTemplateCircleViewModel(ProjectSheetTemplateCircle circulo, TypeLibraryService types)
        {
            _circulo = circulo ?? throw new ArgumentNullException(nameof(circulo));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            AtualizarTipoLinhaAssinado();
        }

        public Guid Id => _circulo.Id;
        public double X => _circulo.X + _previewOffsetX;
        public double Y => _circulo.Y + _previewOffsetY;
        public double ModelX => _circulo.X;
        public double ModelY => _circulo.Y;
        public double Raio => _circulo.Raio;
        public double Diametro => _circulo.Diametro;
        public double Left => X - Raio;
        public double Top => Y - Raio;
        public TipoLinhaAnotativa? TipoLinha => _tipoLinha;
        public string CorLinha => _circulo.Stroke;
        public double StrokeThickness => _circulo.StrokeThickness;
        public string EstiloLinha => TipoLinha?.EstiloLinha ?? "Contínuo";
        public Brush StrokeBrush => CriarBrush(CorLinha);
        public Brush FillBrush => Brushes.Transparent;
        public DoubleCollection? StrokeDashArray => CriarStrokeDashArray(EstiloLinha);
        public bool Visible => _circulo.Visible;
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
            NotificarPosicao();
            OnPropertyChanged(nameof(HasPreviewOffset));
        }

        public void ClearPreviewOffset()
        {
            if (!HasPreviewOffset)
                return;

            _previewOffsetX = 0.0;
            _previewOffsetY = 0.0;
            NotificarPosicao();
            OnPropertyChanged(nameof(HasPreviewOffset));
        }

        public bool Contains(Point position, double tolerance)
        {
            double margem = Math.Max(0.0, tolerance);
            double distancia = Distancia(position, new Point(X, Y));

            return Math.Abs(distancia - Raio) <= margem;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void AtualizarTipoLinhaAssinado()
        {
            if (_tipoLinha != null)
                _tipoLinha.PropertyChanged -= OnTipoLinhaPropertyChanged;

            _tipoLinha = ResolverTipoLinha();

            if (_tipoLinha != null)
                _tipoLinha.PropertyChanged += OnTipoLinhaPropertyChanged;
        }

        private TipoLinhaAnotativa? ResolverTipoLinha()
        {
            if (!_circulo.PossuiTipoLinha)
                return null;

            return _types.TiposLinhasAnotativas.FirstOrDefault(t =>
                string.Equals(t.NomeTipo, _circulo.TipoLinhaNome, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Familia, _circulo.TipoLinhaFamilia, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Categoria, _circulo.TipoLinhaCategoria, StringComparison.OrdinalIgnoreCase)) ?? _types.TipoLinhaAnotativaPadrao;
        }

        private void OnTipoLinhaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.EstiloLinha))
            {
                NotificarEstilo();
            }
        }

        private void NotificarEstilo()
        {
            OnPropertyChanged(nameof(TipoLinha));
            OnPropertyChanged(nameof(CorLinha));
            OnPropertyChanged(nameof(StrokeBrush));
            OnPropertyChanged(nameof(StrokeThickness));
            OnPropertyChanged(nameof(EstiloLinha));
            OnPropertyChanged(nameof(StrokeDashArray));
            OnPropertyChanged(nameof(SelectionStrokeBrush));
            OnPropertyChanged(nameof(SelectionStrokeThickness));
        }

        private void NotificarPosicao()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Left));
            OnPropertyChanged(nameof(Top));
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

        private static DoubleCollection? CriarStrokeDashArray(string estilo)
        {
            string normalizado = NormalizarEstilo(estilo);

            return normalizado switch
            {
                "tracejado" => new DoubleCollection { 6, 4 },
                "tracoponto" => new DoubleCollection { 8, 3, 2, 3 },
                "tracodoispontos" => new DoubleCollection { 8, 3, 2, 3, 2, 3 },
                _ => null
            };
        }

        private static string NormalizarEstilo(string valor)
        {
            return valor
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("ç", "c")
                .Replace("Ç", "c")
                .Replace("ã", "a")
                .Replace("Ã", "a")
                .Replace("í", "i")
                .Replace("Í", "i")
                .ToLowerInvariant()
                .Trim();
        }

        private static double Distancia(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}