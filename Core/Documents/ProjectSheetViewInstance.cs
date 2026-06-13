using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetViewInstance
    {
        public const double DefaultWidth = 260.0;
        public const double DefaultHeight = 180.0;
        public const double MinWidth = 20.0;
        public const double MinHeight = 20.0;

        private double _x;
        private double _y;
        private double _width = DefaultWidth;
        private double _height = DefaultHeight;

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ViewId { get; set; }

        public double X
        {
            get => _x;
            set => _x = NormalizarCoordenada(value);
        }

        public double Y
        {
            get => _y;
            set => _y = NormalizarCoordenada(value);
        }

        public double Width
        {
            get => _width;
            set => _width = NormalizarDimensao(value, MinWidth);
        }

        public double Height
        {
            get => _height;
            set => _height = NormalizarDimensao(value, MinHeight);
        }

        public ProjectSheetViewInstance CriarCopia(bool gerarNovoId)
        {
            return new ProjectSheetViewInstance
            {
                Id = gerarNovoId ? Guid.NewGuid() : Id,
                ViewId = ViewId,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height
            };
        }

        public bool IsValid => Id != Guid.Empty && ViewId != Guid.Empty;

        private static double NormalizarCoordenada(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) ? 0.0 : valor;
        }

        private static double NormalizarDimensao(double valor, double minimo)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < minimo
                ? minimo
                : valor;
        }
    }
}