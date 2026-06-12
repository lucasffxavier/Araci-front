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
    public sealed class ProjectSheetTemplateRectangleViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetTemplateRectangle _retangulo;
        private readonly TypeLibraryService _types;
        private TipoLinhaAnotativa? _tipoLinha;
        private bool _isSelected;
        private double _previewOffsetX;
        private double _previewOffsetY;
        private bool _hasPreviewGeometry;
        private double _previewX;
        private double _previewY;
        private double _previewLargura;
        private double _previewAltura;

        public ProjectSheetTemplateRectangleViewModel(ProjectSheetTemplateRectangle retangulo)
            : this(retangulo, new TypeLibraryService())
        {
        }

        public ProjectSheetTemplateRectangleViewModel(ProjectSheetTemplateRectangle retangulo, TypeLibraryService types)
        {
            _retangulo = retangulo ?? throw new ArgumentNullException(nameof(retangulo));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            AtualizarTipoLinhaAssinado();
        }

        public Guid Id => _retangulo.Id;
        public double X => _hasPreviewGeometry ? _previewX : _retangulo.X + _previewOffsetX;
        public double Y => _hasPreviewGeometry ? _previewY : _retangulo.Y + _previewOffsetY;
        public double ModelX => _retangulo.X;
        public double ModelY => _retangulo.Y;
        public double Largura => _hasPreviewGeometry ? _previewLargura : _retangulo.Largura;
        public double Altura => _hasPreviewGeometry ? _previewAltura : _retangulo.Altura;
        public double ModelLargura => _retangulo.Largura;
        public double ModelAltura => _retangulo.Altura;
        public TipoLinhaAnotativa? TipoLinha => _tipoLinha;
        public string CorLinha => _retangulo.Stroke;
        public double StrokeThickness => _retangulo.StrokeThickness;
        public string EstiloLinha => TipoLinha?.EstiloLinha ?? "Contínuo";
        public Brush StrokeBrush => CriarBrush(CorLinha);
        public Brush FillBrush => Brushes.Transparent;
        public DoubleCollection? StrokeDashArray => CriarStrokeDashArray(EstiloLinha);
        public bool Visible => _retangulo.Visible;
        public bool HasPreviewOffset => Math.Abs(_previewOffsetX) > 0.0001 || Math.Abs(_previewOffsetY) > 0.0001;
        public bool HasPreviewGeometry => _hasPreviewGeometry;

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
            if (_hasPreviewGeometry)
                ClearPreviewGeometry();

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

        public void SetPreviewGeometry(double x, double y, double largura, double altura)
        {
            if (_hasPreviewGeometry &&
                Math.Abs(_previewX - x) < 0.0001 &&
                Math.Abs(_previewY - y) < 0.0001 &&
                Math.Abs(_previewLargura - largura) < 0.0001 &&
                Math.Abs(_previewAltura - altura) < 0.0001)
            {
                return;
            }

            _hasPreviewGeometry = true;
            _previewOffsetX = 0.0;
            _previewOffsetY = 0.0;
            _previewX = x;
            _previewY = y;
            _previewLargura = largura;
            _previewAltura = altura;
            NotificarGeometria();
            OnPropertyChanged(nameof(HasPreviewOffset));
            OnPropertyChanged(nameof(HasPreviewGeometry));
        }

        public void ClearPreviewGeometry()
        {
            if (!_hasPreviewGeometry)
                return;

            _hasPreviewGeometry = false;
            _previewX = 0.0;
            _previewY = 0.0;
            _previewLargura = 0.0;
            _previewAltura = 0.0;
            NotificarGeometria();
            OnPropertyChanged(nameof(HasPreviewGeometry));
        }

        public bool Contains(Point position, double tolerance)
        {
            double margem = Math.Max(0.0, tolerance);
            Rect bounds = new Rect(X, Y, Math.Max(0.0, Largura), Math.Max(0.0, Altura));
            Rect hitBounds = bounds;
            hitBounds.Inflate(margem, margem);

            if (!hitBounds.Contains(position))
                return false;

            Point topLeft = new(bounds.Left, bounds.Top);
            Point topRight = new(bounds.Right, bounds.Top);
            Point bottomRight = new(bounds.Right, bounds.Bottom);
            Point bottomLeft = new(bounds.Left, bounds.Bottom);

            double menor = double.MaxValue;
            menor = Math.Min(menor, DistanciaPontoSegmento(position, topLeft, topRight));
            menor = Math.Min(menor, DistanciaPontoSegmento(position, topRight, bottomRight));
            menor = Math.Min(menor, DistanciaPontoSegmento(position, bottomRight, bottomLeft));
            menor = Math.Min(menor, DistanciaPontoSegmento(position, bottomLeft, topLeft));

            return menor <= margem;
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
            if (!_retangulo.PossuiTipoLinha)
                return null;

            return _types.TiposLinhasAnotativas.FirstOrDefault(t =>
                string.Equals(t.NomeTipo, _retangulo.TipoLinhaNome, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Familia, _retangulo.TipoLinhaFamilia, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Categoria, _retangulo.TipoLinhaCategoria, StringComparison.OrdinalIgnoreCase)) ?? _types.TipoLinhaAnotativaPadrao;
        }

        private void OnTipoLinhaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.EstiloLinha))
            {
                NotificarEstilo();
            }
        }

        private void NotificarPosicao()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        private void NotificarGeometria()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Largura));
            OnPropertyChanged(nameof(Altura));
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

        private static double DistanciaPontoSegmento(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;

            if (lengthSquared <= double.Epsilon)
                return Distancia(p, a);

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));
            var projection = new Point(a.X + t * dx, a.Y + t * dy);
            return Distancia(p, projection);
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