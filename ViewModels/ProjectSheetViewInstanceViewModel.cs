using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetViewInstanceViewModel : INotifyPropertyChanged
    {
        private const double Epsilon = 0.000001;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private double _sheetOriginOffsetX;
        private double _sheetOriginOffsetY;

        public ProjectSheetViewInstanceViewModel(ProjectSheetViewInstance instance, ProjectView? view)
        {
            ArgumentNullException.ThrowIfNull(instance);

            Id = instance.Id;
            ViewId = instance.ViewId;
            ViewName = string.IsNullOrWhiteSpace(view?.Nome) ? "Vista nao encontrada" : view!.Nome;
            Scale = string.IsNullOrWhiteSpace(view?.Escala) ? "1:100" : view!.Escala;
            Discipline = view?.Disciplina.ToString() ?? string.Empty;
            CropWidth = Math.Max(ProjectView.MinRecorteDimension, view?.RecorteLargura ?? ProjectView.DefaultRecorteLargura);
            CropHeight = Math.Max(ProjectView.MinRecorteDimension, view?.RecorteAltura ?? ProjectView.DefaultRecorteAltura);
            IsCropped = view?.RecortarVista == true;
            _x = NormalizePosition(instance.X);
            _y = NormalizePosition(instance.Y);
            _width = NormalizeDimension(instance.Width, ProjectSheetViewInstance.MinWidth);
            _height = NormalizeDimension(instance.Height, ProjectSheetViewInstance.MinHeight);
        }

        public Guid Id { get; }
        public Guid ViewId { get; }
        public string ViewName { get; }
        public string Scale { get; }
        public string Discipline { get; }
        public double CropWidth { get; }
        public double CropHeight { get; }
        public bool IsCropped { get; }
        public string Title => $"Vista: {ViewName}";
        public string Subtitle => string.IsNullOrWhiteSpace(Discipline) ? $"Escala {Scale}" : $"{Discipline} · Escala {Scale}";
        public string CropText => $"Recorte {FormatarNumero(CropWidth)} × {FormatarNumero(CropHeight)}";
        public string CropStatusText => IsCropped ? "Recorte ativo" : "Recorte desativado";
        public double ViewX => X + SheetOriginOffsetX;
        public double ViewY => Y + SheetOriginOffsetY;

        public double SheetOriginOffsetX
        {
            get => _sheetOriginOffsetX;
            private set
            {
                if (Math.Abs(_sheetOriginOffsetX - value) < Epsilon)
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
                if (Math.Abs(_sheetOriginOffsetY - value) < Epsilon)
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
                if (Math.Abs(_x - value) < Epsilon)
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
                if (Math.Abs(_y - value) < Epsilon)
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
                double normalized = NormalizeDimension(value, ProjectSheetViewInstance.MinWidth);

                if (Math.Abs(_width - normalized) < Epsilon)
                    return;

                _width = normalized;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get => _height;
            private set
            {
                double normalized = NormalizeDimension(value, ProjectSheetViewInstance.MinHeight);

                if (Math.Abs(_height - normalized) < Epsilon)
                    return;

                _height = normalized;
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
            Width = NormalizeDimension(width, ProjectSheetViewInstance.MinWidth);
            Height = NormalizeDimension(height, ProjectSheetViewInstance.MinHeight);
        }

        private static string FormatarNumero(double valor)
        {
            return valor.ToString("0.###", CultureInfo.CurrentCulture);
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