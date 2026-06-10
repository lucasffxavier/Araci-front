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
        public double X => _retangulo.X;
        public double Y => _retangulo.Y;
        public double Largura => _retangulo.Largura;
        public double Altura => _retangulo.Altura;
        public TipoLinhaAnotativa? TipoLinha => _tipoLinha;
        public string CorLinha => _retangulo.Stroke;
        public double StrokeThickness => _retangulo.StrokeThickness;
        public string EstiloLinha => TipoLinha?.EstiloLinha ?? "Contínuo";
        public Brush StrokeBrush => CriarBrush(CorLinha);
        public Brush FillBrush => Brushes.Transparent;
        public DoubleCollection? StrokeDashArray => CriarStrokeDashArray(EstiloLinha);
        public bool Visible => _retangulo.Visible;

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

        private void NotificarEstilo()
        {
            OnPropertyChanged(nameof(TipoLinha));
            OnPropertyChanged(nameof(CorLinha));
            OnPropertyChanged(nameof(StrokeBrush));
            OnPropertyChanged(nameof(StrokeThickness));
            OnPropertyChanged(nameof(EstiloLinha));
            OnPropertyChanged(nameof(StrokeDashArray));
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