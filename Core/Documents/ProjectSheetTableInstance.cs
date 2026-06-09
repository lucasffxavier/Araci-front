using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetTableInstance
    {
        public const double DefaultWidth = 180.0;
        public const double DefaultHeight = 80.0;
        public const double MinWidth = 20.0;
        public const double MinHeight = 20.0;

        private double _x;
        private double _y;
        private double _width = DefaultWidth;
        private double _height = DefaultHeight;
        private int _rowStartIndex;
        private int? _rowCount;

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TableId { get; set; }

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

        public int RowStartIndex
        {
            get => _rowStartIndex;
            set => _rowStartIndex = value < 0 ? 0 : value;
        }

        public int? RowCount
        {
            get => _rowCount;
            set => _rowCount = value.HasValue && value.Value > 0 ? value.Value : null;
        }

        public ProjectSheetTableInstance CriarCopia(bool gerarNovoId)
        {
            return new ProjectSheetTableInstance
            {
                Id = gerarNovoId ? Guid.NewGuid() : Id,
                TableId = TableId,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                RowStartIndex = RowStartIndex,
                RowCount = RowCount
            };
        }

        public bool IsValid => Id != Guid.Empty && TableId != Guid.Empty;

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
