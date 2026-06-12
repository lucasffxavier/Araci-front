using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Araci.Applications.Projects.Tables;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTableInstanceViewModel : INotifyPropertyChanged
    {
        private const double MinimumColumnWidth = 44.0;

        private readonly ProjectTableDisplaySettings _exibicao;
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
            ProjectTableDataResult? tableData = null,
            ProjectTableDisplaySettings? exibicao = null)
        {
            ArgumentNullException.ThrowIfNull(instance);

            _exibicao = NormalizarExibicao(exibicao);
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
            Rows = BuildVisibleRows(tableData, RowStartIndex, RowCount, _exibicao);
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
        public bool ExibirTitulo => _exibicao.ExibirTitulo;
        public bool ExibirCabecalho => _exibicao.ExibirCabecalho;
        public double TitleHeight => ExibirTitulo ? _exibicao.AlturaTitulo : 0.0;
        public double HeaderRowHeight => ExibirCabecalho ? _exibicao.AlturaCabecalho : 0.0;
        public double BodyRowHeight => _exibicao.AlturaLinhaCorpo;
        public double RowsContentHeight => HasRows ? BodyRowHeight * Rows.Count : 0.0;
        public double TableContentHeight => HasRenderableTable ? TitleHeight + HeaderRowHeight + RowsContentHeight : Height;
        public double RenderedHeight => HasRenderableTable ? Math.Max(ProjectSheetTableInstance.MinHeight, Math.Min(Height, TableContentHeight)) : Height;
        public double BodyViewportHeight => Math.Max(0.0, RenderedHeight - TitleHeight);
        public double RowsViewportHeight => Math.Max(0.0, RenderedHeight - TitleHeight - HeaderRowHeight);
        public double ViewX => X + SheetOriginOffsetX;
        public double ViewY => Y + SheetOriginOffsetY;
        public FontFamily TitleFontFamily => new(_exibicao.FonteTitulo);
        public FontFamily HeaderFontFamily => new(_exibicao.FonteCabecalho);
        public FontFamily BodyFontFamily => new(_exibicao.FonteCorpo);
        public double TitleFontSize => _exibicao.TamanhoFonteTitulo;
        public double HeaderFontSize => _exibicao.TamanhoFonteCabecalho;
        public double BodyFontSize => _exibicao.TamanhoFonteCorpo;
        public FontWeight TitleFontWeight => _exibicao.TituloNegrito ? FontWeights.SemiBold : FontWeights.Normal;
        public FontWeight HeaderFontWeight => _exibicao.CabecalhoNegrito ? FontWeights.SemiBold : FontWeights.Normal;
        public Brush TitleForegroundBrush => CriarBrush(_exibicao.CorTextoTitulo, ProjectTableDisplaySettings.DefaultTitleTextColor);
        public Brush TitleBackgroundBrush => CriarBrush(_exibicao.CorFundoTitulo, ProjectTableDisplaySettings.DefaultTitleBackgroundColor);
        public Brush HeaderForegroundBrush => CriarBrush(_exibicao.CorTextoCabecalho, ProjectTableDisplaySettings.DefaultHeaderTextColor);
        public Brush HeaderBackgroundBrush => CriarBrush(_exibicao.CorFundoCabecalho, ProjectTableDisplaySettings.DefaultHeaderBackgroundColor);
        public Brush BodyForegroundBrush => CriarBrush(_exibicao.CorTextoCorpo, ProjectTableDisplaySettings.DefaultBodyTextColor);
        public Brush BodyBackgroundBrush => CriarBrush(_exibicao.CorFundoCorpo, ProjectTableDisplaySettings.DefaultBodyBackgroundColor);
        public Brush AlternateRowBackgroundBrush => CriarBrush(_exibicao.CorLinhaAlternada, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor);
        public Brush GridBrush => CriarBrush(_exibicao.CorGrade, ProjectTableDisplaySettings.DefaultGridColor);
        public Brush OutlineBrush => CriarBrush(_exibicao.CorContorno, ProjectTableDisplaySettings.DefaultOutlineColor);
        public Thickness GridBorderThickness => _exibicao.ExibirLinhasGrade ? new Thickness(0, 0, _exibicao.EspessuraGrade, _exibicao.EspessuraGrade) : new Thickness(0);
        public Thickness OutlineBorderThickness => _exibicao.ExibirContornoExterno ? new Thickness(_exibicao.EspessuraContorno) : new Thickness(0);
        public TextAlignment TitleTextAlignment => ConverterAlinhamento(_exibicao.AlinhamentoTitulo);
        public TextAlignment HeaderTextAlignment => ConverterAlinhamento(_exibicao.AlinhamentoCabecalho);
        public TextAlignment BodyTextAlignment => ConverterAlinhamento(_exibicao.AlinhamentoCorpo);

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
                OnPropertyChanged(nameof(TableContentHeight));
                OnPropertyChanged(nameof(RenderedHeight));
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
            int? rowCount,
            ProjectTableDisplaySettings exibicao)
        {
            if (tableData == null || tableData.Columns.Count == 0)
                return new List<ProjectSheetTableRowViewModel>();

            int startIndex = rowStartIndex < 0 ? 0 : rowStartIndex;
            IEnumerable<ProjectTableDataRow> rows = tableData.Rows.Skip(startIndex);

            if (rowCount.HasValue)
                rows = rows.Take(rowCount.Value);

            return rows
                .Select((row, index) => new ProjectSheetTableRowViewModel(row, index, exibicao))
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

        private static ProjectTableDisplaySettings NormalizarExibicao(ProjectTableDisplaySettings? valor)
        {
            return valor?.CriarCopia() ?? new ProjectTableDisplaySettings();
        }

        private static Brush CriarBrush(string? valor, string fallback)
        {
            try
            {
                object? convertido = ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(valor) ? fallback : valor);
                return convertido is Color cor ? new SolidColorBrush(cor) : new SolidColorBrush(Colors.Black);
            }
            catch
            {
                object? convertido = ColorConverter.ConvertFromString(fallback);
                return convertido is Color cor ? new SolidColorBrush(cor) : new SolidColorBrush(Colors.Black);
            }
        }

        private static TextAlignment ConverterAlinhamento(ProjectTableTextAlignment alinhamento)
        {
            return alinhamento switch
            {
                ProjectTableTextAlignment.Centro => TextAlignment.Center,
                ProjectTableTextAlignment.Direita => TextAlignment.Right,
                _ => TextAlignment.Left
            };
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
        public ProjectSheetTableRowViewModel(ProjectTableDataRow row, int visualIndex, ProjectTableDisplaySettings exibicao)
        {
            ElementoId = row.ElementoId;
            Cells = row.Cells
                .Select(cell => new ProjectSheetTableCellViewModel(cell.DisplayValue))
                .ToList();
            BackgroundBrush = exibicao.UsarLinhasAlternadas && visualIndex % 2 == 1
                ? CriarBrush(exibicao.CorLinhaAlternada, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor)
                : CriarBrush(exibicao.CorFundoCorpo, ProjectTableDisplaySettings.DefaultBodyBackgroundColor);
        }

        public Guid ElementoId { get; }
        public IReadOnlyList<ProjectSheetTableCellViewModel> Cells { get; }
        public Brush BackgroundBrush { get; }

        private static Brush CriarBrush(string? valor, string fallback)
        {
            try
            {
                object? convertido = ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(valor) ? fallback : valor);
                return convertido is Color cor ? new SolidColorBrush(cor) : new SolidColorBrush(Colors.White);
            }
            catch
            {
                object? convertido = ColorConverter.ConvertFromString(fallback);
                return convertido is Color cor ? new SolidColorBrush(cor) : new SolidColorBrush(Colors.White);
            }
        }
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