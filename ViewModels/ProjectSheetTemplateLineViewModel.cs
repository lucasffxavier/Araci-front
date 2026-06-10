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
    public enum ProjectSheetTemplateLineEndpoint
    {
        Start,
        End
    }

    public sealed class ProjectSheetTemplateLineViewModel : INotifyPropertyChanged
    {
        private const double EndpointHandleSizeValue = 10.0;

        private readonly ProjectSheetTemplateLine _linha;
        private readonly TypeLibraryService _types;
        private TipoLinhaAnotativa? _tipoLinha;
        private bool _isSelected;
        private double _previewOffsetX;
        private double _previewOffsetY;
        private bool _hasEndpointPreview;
        private double _previewX1;
        private double _previewY1;
        private double _previewX2;
        private double _previewY2;

        public ProjectSheetTemplateLineViewModel(ProjectSheetTemplateLine linha)
            : this(linha, new TypeLibraryService())
        {
        }

        public ProjectSheetTemplateLineViewModel(ProjectSheetTemplateLine linha, TypeLibraryService types)
        {
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            AtualizarTipoLinhaAssinado();
        }

        public Guid Id => _linha.Id;
        public double X1 => _hasEndpointPreview ? _previewX1 : _linha.X1 + _previewOffsetX;
        public double Y1 => _hasEndpointPreview ? _previewY1 : _linha.Y1 + _previewOffsetY;
        public double X2 => _hasEndpointPreview ? _previewX2 : _linha.X2 + _previewOffsetX;
        public double Y2 => _hasEndpointPreview ? _previewY2 : _linha.Y2 + _previewOffsetY;
        public double ModelX1 => _linha.X1;
        public double ModelY1 => _linha.Y1;
        public double ModelX2 => _linha.X2;
        public double ModelY2 => _linha.Y2;
        public TipoLinhaAnotativa? TipoLinha => _tipoLinha;
        public string CorLinha => _linha.Stroke;
        public double StrokeThickness => _linha.StrokeThickness;
        public string EstiloLinha => TipoLinha?.EstiloLinha ?? "Contínuo";
        public Brush StrokeBrush => CriarBrush(CorLinha);
        public DoubleCollection? StrokeDashArray => CriarStrokeDashArray(EstiloLinha);
        public bool Visible => _linha.Visible;
        public bool HasPreviewOffset => Math.Abs(_previewOffsetX) > 0.0001 || Math.Abs(_previewOffsetY) > 0.0001;
        public bool HasEndpointPreview => _hasEndpointPreview;
        public double EndpointHandleSize => EndpointHandleSizeValue;
        public double StartHandleLeft => X1 - EndpointHandleSizeValue / 2.0;
        public double StartHandleTop => Y1 - EndpointHandleSizeValue / 2.0;
        public double EndHandleLeft => X2 - EndpointHandleSizeValue / 2.0;
        public double EndHandleTop => Y2 - EndpointHandleSizeValue / 2.0;
        public bool EndpointHandlesVisible => IsSelected;
        public Visibility EndpointHandlesVisibility => EndpointHandlesVisible ? Visibility.Visible : Visibility.Collapsed;

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
                OnPropertyChanged(nameof(EndpointHandlesVisible));
                OnPropertyChanged(nameof(EndpointHandlesVisibility));
            }
        }

        public Brush SelectionStrokeBrush => IsSelected ? Brushes.DodgerBlue : StrokeBrush;
        public double SelectionStrokeThickness => IsSelected ? Math.Max(StrokeThickness + 3.0, 4.0) : StrokeThickness;

        public void SetPreviewOffset(double deltaX, double deltaY)
        {
            if (_hasEndpointPreview)
                ClearEndpointPreview();

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

        public void SetEndpointPreview(ProjectSheetTemplateLineEndpoint endpoint, double x, double y)
        {
            if (!_hasEndpointPreview)
            {
                _previewX1 = _linha.X1;
                _previewY1 = _linha.Y1;
                _previewX2 = _linha.X2;
                _previewY2 = _linha.Y2;
            }

            if (endpoint == ProjectSheetTemplateLineEndpoint.Start)
            {
                _previewX1 = x;
                _previewY1 = y;
            }
            else
            {
                _previewX2 = x;
                _previewY2 = y;
            }

            _hasEndpointPreview = true;
            _previewOffsetX = 0.0;
            _previewOffsetY = 0.0;
            NotificarCoordenadas();
            OnPropertyChanged(nameof(HasPreviewOffset));
            OnPropertyChanged(nameof(HasEndpointPreview));
        }

        public void SetPreviewCoordinates(double x1, double y1, double x2, double y2)
        {
            _previewX1 = x1;
            _previewY1 = y1;
            _previewX2 = x2;
            _previewY2 = y2;
            _hasEndpointPreview = true;
            _previewOffsetX = 0.0;
            _previewOffsetY = 0.0;
            NotificarCoordenadas();
            OnPropertyChanged(nameof(HasPreviewOffset));
            OnPropertyChanged(nameof(HasEndpointPreview));
        }

        public void ClearEndpointPreview()
        {
            if (!_hasEndpointPreview)
                return;

            _hasEndpointPreview = false;
            _previewX1 = 0.0;
            _previewY1 = 0.0;
            _previewX2 = 0.0;
            _previewY2 = 0.0;
            NotificarCoordenadas();
            OnPropertyChanged(nameof(HasEndpointPreview));
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
            if (!_linha.PossuiTipoLinha)
                return null;

            return _types.TiposLinhasAnotativas.FirstOrDefault(t =>
                string.Equals(t.NomeTipo, _linha.TipoLinhaNome, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Familia, _linha.TipoLinhaFamilia, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Categoria, _linha.TipoLinhaCategoria, StringComparison.OrdinalIgnoreCase)) ?? _types.TipoLinhaAnotativaPadrao;
        }

        private void OnTipoLinhaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.EstiloLinha))
            {
                NotificarEstilo();
            }
        }

        private void NotificarCoordenadas()
        {
            OnPropertyChanged(nameof(X1));
            OnPropertyChanged(nameof(Y1));
            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
            OnPropertyChanged(nameof(StartHandleLeft));
            OnPropertyChanged(nameof(StartHandleTop));
            OnPropertyChanged(nameof(EndHandleLeft));
            OnPropertyChanged(nameof(EndHandleTop));
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}