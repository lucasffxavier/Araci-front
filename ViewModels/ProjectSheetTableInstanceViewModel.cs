using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Araci.Applications.Projects.Tables;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTableInstanceViewModel : INotifyPropertyChanged
    {
        private const double MinimumColumnWidth = 44.0;
        private const double DefaultTitleHeight = 32.0;
        private const double DefaultHeaderRowHeight = 26.0;
        private const double DefaultBodyRowHeight = 24.0;

        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private double _sheetOriginOffsetX;
        private double _sheetOriginOffsetY;
        private bool _isSelected;

        public ProjectSheetTableInstanceViewModel(
            ProjectSheetTableInstance instance,
            string tableName,
            ProjectTableDataResult? tableData = null)
        {
            ArgumentNullException.ThrowIfNull(instance);

            Id = instance.Id;
            TableId = instance.TableId;
            TableName = string.IsNullOrWhiteSpace(tableName) ? "Tabela sem nome" : tableName;
            _x = NormalizePosition(instance.X);
            _y = NormalizePosition(instance.Y);
            _width = NormalizeDimension(instance.Width, ProjectSheetTableInstance.MinWidth);
            _height = NormalizeDimension(instance.Height, ProjectSheetTableInstance.MinHeight);
            Columns = tableData?.Columns.ToList() ?? new List<ProjectTableDataColumn>();
            RowStartIndex = instance.RowStartIndex;
            RowCount = instance.RowCount;
            Rows = BuildVisibleRows(tableData, RowStartIndex, RowCount);
            EmptyDataMessage = tableData == null
                ? "Tabela nao encontrada"
                : Columns.Count == 0
                    ? "Nenhum campo selecionado"
                    : Rows.Count == 0
                        ? "Nenhum item encontrado"
                        : string.Empty;
        }

        public Guid Id { get; }
        public Guid TableId { get; }
        public string TableName { get; }
        public IReadOnlyList<ProjectTableDataColumn> Columns { get; }
        public IReadOnlyList<ProjectSheetTableRowViewModel> Rows { get; }
        public int RowStartIndex { get; }
        public int? RowCount { get; }
        public bool HasColumns => Columns.Count > 0;
        public bool HasRows => Rows.Count > 0;
        public bool HasRenderableTable => HasColumns && HasRows;
        public bool CanSplit => HasRenderableTable && Rows.Count > 1 && RowStartIndex == 0 && !RowCount.HasValue;
        public bool HasEmptyDataMessage => !string.IsNullOrWhiteSpace(EmptyDataMessage);
        public string EmptyDataMessage { get; }
        public int ColumnCount => Columns.Count;
        public double ColumnWidth => CalcularLarguraColuna(Width, Columns.Count);
        public double TableGridWidth => HasColumns ? ColumnWidth * Columns.Count : Math.Max(0.0, Width);
        public double TitleHeight => DefaultTitleHeight;
        public double HeaderRowHeight => DefaultHeaderRowHeight;
        public double BodyRowHeight => DefaultBodyRowHeight;
        public double BodyViewportHeight => Math.Max(0.0, Height - TitleHeight);
        public double RowsViewportHeight => Math.Max(0.0, Height - TitleHeight - HeaderRowHeight);
        public double ViewX => X + SheetOriginOffsetX;
        public double ViewY => Y + SheetOriginOffsetY;

        public double SheetOriginOffsetX
        {
            get => _sheetOriginOffsetX;
            private set
            {
                if (Math.Abs(_sheetOriginOffsetX - value) < 0.000001)
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
                if (Math.Abs(_sheetOriginOffsetY - value) < 0.000001)
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
                if (Math.Abs(_x - value) < 0.000001)
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
                if (Math.Abs(_y - value) < 0.000001)
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
                if (Math.Abs(_width - value) < 0.000001)
                    return;

                _width = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnWidth));
                OnPropertyChanged(nameof(TableGridWidth));
            }
        }

        public double Height
        {
            get => _height;
            private set
            {
                if (Math.Abs(_height - value) < 0.000001)
                    return;

                _height = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BodyViewportHeight));
                OnPropertyChanged(nameof(RowsViewportHeight));
            }
        }

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
            Width = NormalizeDimension(width, ProjectSheetTableInstance.MinWidth);
            Height = NormalizeDimension(height, ProjectSheetTableInstance.MinHeight);
        }

        private static IReadOnlyList<ProjectSheetTableRowViewModel> BuildVisibleRows(
            ProjectTableDataResult? tableData,
            int rowStartIndex,
            int? rowCount)
        {
            if (tableData == null || tableData.Columns.Count == 0)
                return new List<ProjectSheetTableRowViewModel>();

            int startIndex = rowStartIndex < 0 ? 0 : rowStartIndex;
            IEnumerable<ProjectTableDataRow> rows = tableData.Rows.Skip(startIndex);

            if (rowCount.HasValue)
                rows = rows.Take(rowCount.Value);

            return rows
                .Select(row => new ProjectSheetTableRowViewModel(row))
                .ToList();
        }

        private static double CalcularLarguraColuna(double width, int columnCount)
        {
            if (columnCount <= 0)
                return NormalizeDimension(width, ProjectSheetTableInstance.MinWidth);

            double safeWidth = NormalizeDimension(width, ProjectSheetTableInstance.MinWidth);
            double widthPerColumn = safeWidth / columnCount;

            return widthPerColumn < MinimumColumnWidth
                ? MinimumColumnWidth
                : widthPerColumn;
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

    public sealed class ProjectSheetTableRowViewModel
    {
        public ProjectSheetTableRowViewModel(ProjectTableDataRow row)
        {
            ElementoId = row.ElementoId;
            Cells = row.Cells
                .Select(cell => new ProjectSheetTableCellViewModel(cell.DisplayValue))
                .ToList();
        }

        public Guid ElementoId { get; }
        public IReadOnlyList<ProjectSheetTableCellViewModel> Cells { get; }
    }

    public sealed class ProjectSheetTableCellViewModel
    {
        public ProjectSheetTableCellViewModel(string displayValue)
        {
            DisplayValue = displayValue ?? string.Empty;
        }

        public string DisplayValue { get; }
    }
}