using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        private const double HorizontalTextPadding = 14.0;
        private const double ColumnSafetyPadding = 8.0;
        private const double Epsilon = 0.000001;

        private readonly ProjectTableDisplaySettings _exibicao;
        private readonly IReadOnlyList<ProjectTableDataColumn> _sourceColumns;
        private readonly IReadOnlyList<ProjectTableDataRow> _visibleDataRows;
        private IReadOnlyList<ProjectSheetTableColumnViewModel> _renderColumns = new List<ProjectSheetTableColumnViewModel>();
        private IReadOnlyList<ProjectSheetTableRowViewModel> _rows = new List<ProjectSheetTableRowViewModel>();
        private double _minimumTableWidth;
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
            _height = NormalizeDimension(instance.Height, ProjectSheetTableInstance.MinHeight);
            _sourceColumns = tableData?.Columns.ToList() ?? new List<ProjectTableDataColumn>();
            RowStartIndex = instance.RowStartIndex;
            RowCount = instance.RowCount;
            _visibleDataRows = BuildVisibleDataRows(tableData, RowStartIndex, RowCount);
            _minimumTableWidth = CalcularLarguraMinimaTabela(_sourceColumns, _visibleDataRows, _exibicao);
            _width = NormalizeDimension(instance.Width, MinimumTableWidth);
            RebuildTableLayout(false);
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
        public IReadOnlyList<ProjectTableDataColumn> Columns => _sourceColumns;
        public IReadOnlyList<ProjectSheetTableColumnViewModel> RenderColumns => _renderColumns;
        public IReadOnlyList<ProjectSheetTableRowViewModel> Rows => _rows;
        public int RowStartIndex { get; }
        public int? RowCount { get; }
        public bool HasColumns => Columns.Count > 0;
        public bool HasRows => Rows.Count > 0;
        public bool HasRenderableTable => HasColumns && HasRows;
        public bool CanSplit => HasRenderableTable && Rows.Count > 1 && RowStartIndex == 0 && !RowCount.HasValue;
        public bool HasEmptyDataMessage => !string.IsNullOrWhiteSpace(EmptyDataMessage);
        public string EmptyDataMessage { get; }
        public int ColumnCount => Columns.Count;
        public double MinimumTableWidth => HasColumns ? Math.Max(ProjectSheetTableInstance.MinWidth, _minimumTableWidth) : ProjectSheetTableInstance.MinWidth;
        public double ColumnWidth => ColumnCount <= 0 ? Math.Max(0.0, Width) : TableGridWidth / ColumnCount;
        public double TableGridWidth => HasColumns ? RenderColumns.Sum(c => c.Width) : Math.Max(0.0, Width);
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
        public Brush AlternateRowBackgroundBrush => _exibicao.UsarLinhasAlternadas
            ? CriarBrush(_exibicao.CorLinhaAlternada, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor)
            : BodyBackgroundBrush;
        public Brush GridBrush => CriarBrush(_exibicao.CorGrade, ProjectTableDisplaySettings.DefaultGridColor);
        public Brush OutlineBrush => CriarBrush(_exibicao.CorContorno, ProjectTableDisplaySettings.DefaultOutlineColor);
        public Thickness GridBorderThickness => _exibicao.ExibirLinhasGrade ? new Thickness(0, 0, _exibicao.EspessuraGrade, _exibicao.EspessuraGrade) : new Thickness(0);
        public Thickness GridSeparatorBorderThickness => _exibicao.ExibirLinhasGrade ? new Thickness(0, 0, 0, _exibicao.EspessuraGrade) : new Thickness(0);
        public Thickness OutlineBorderThickness => _exibicao.ExibirContornoExterno ? new Thickness(_exibicao.EspessuraContorno) : new Thickness(0);
        public TextAlignment TitleTextAlignment => ConverterAlinhamento(_exibicao.AlinhamentoTitulo);
        public TextAlignment HeaderTextAlignment => ConverterAlinhamento(_exibicao.AlinhamentoCabecalho);
        public TextAlignment BodyTextAlignment => ConverterAlinhamento(_exibicao.AlinhamentoCorpo);

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
                double normalized = NormalizeDimension(value, MinimumTableWidth);

                if (Math.Abs(_width - normalized) < Epsilon)
                    return;

                _width = normalized;
                RebuildTableLayout(true);
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
                if (Math.Abs(_height - value) < Epsilon)
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
            Width = NormalizeDimension(width, MinimumTableWidth);
            Height = NormalizeDimension(height, ProjectSheetTableInstance.MinHeight);
        }

        private void RebuildTableLayout(bool notify)
        {
            _renderColumns = BuildRenderColumns(_sourceColumns, _visibleDataRows, _exibicao, Width);
            _rows = BuildVisibleRows(_visibleDataRows, _renderColumns, _exibicao);

            if (!notify)
                return;

            OnPropertyChanged(nameof(RenderColumns));
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(HasRows));
            OnPropertyChanged(nameof(HasRenderableTable));
            OnPropertyChanged(nameof(CanSplit));
            OnPropertyChanged(nameof(RowsContentHeight));
            OnPropertyChanged(nameof(TableContentHeight));
            OnPropertyChanged(nameof(RenderedHeight));
            OnPropertyChanged(nameof(BodyViewportHeight));
            OnPropertyChanged(nameof(RowsViewportHeight));
        }

        private static IReadOnlyList<ProjectTableDataRow> BuildVisibleDataRows(
            ProjectTableDataResult? tableData,
            int rowStartIndex,
            int? rowCount)
        {
            if (tableData == null || tableData.Columns.Count == 0)
                return new List<ProjectTableDataRow>();

            int startIndex = rowStartIndex < 0 ? 0 : rowStartIndex;
            IEnumerable<ProjectTableDataRow> rows = tableData.Rows.Skip(startIndex);

            if (rowCount.HasValue)
                rows = rows.Take(rowCount.Value);

            return rows.ToList();
        }

        private static IReadOnlyList<ProjectSheetTableColumnViewModel> BuildRenderColumns(
            IReadOnlyList<ProjectTableDataColumn> columns,
            IReadOnlyList<ProjectTableDataRow> rows,
            ProjectTableDisplaySettings exibicao,
            double width)
        {
            if (columns.Count == 0)
                return new List<ProjectSheetTableColumnViewModel>();

            double[] minWidths = columns
                .Select((column, index) => CalcularLarguraMinimaColuna(column, index, rows, exibicao))
                .ToArray();

            double minTotal = minWidths.Sum();
            double safeWidth = NormalizeDimension(width, minTotal);
            double extraPerColumn = safeWidth > minTotal
                ? (safeWidth - minTotal) / columns.Count
                : 0.0;

            return columns
                .Select((column, index) => new ProjectSheetTableColumnViewModel(
                    column,
                    minWidths[index] + extraPerColumn))
                .ToList();
        }

        private static IReadOnlyList<ProjectSheetTableRowViewModel> BuildVisibleRows(
            IReadOnlyList<ProjectTableDataRow> rows,
            IReadOnlyList<ProjectSheetTableColumnViewModel> columns,
            ProjectTableDisplaySettings exibicao)
        {
            return rows
                .Select((row, index) => new ProjectSheetTableRowViewModel(row, index, exibicao, columns))
                .ToList();
        }

        private static double CalcularLarguraMinimaTabela(
            IReadOnlyList<ProjectTableDataColumn> columns,
            IReadOnlyList<ProjectTableDataRow> rows,
            ProjectTableDisplaySettings exibicao)
        {
            if (columns.Count == 0)
                return ProjectSheetTableInstance.MinWidth;

            double total = columns
                .Select((column, index) => CalcularLarguraMinimaColuna(column, index, rows, exibicao))
                .Sum();

            return Math.Max(ProjectSheetTableInstance.MinWidth, total);
        }

        private static double CalcularLarguraMinimaColuna(
            ProjectTableDataColumn column,
            int columnIndex,
            IReadOnlyList<ProjectTableDataRow> rows,
            ProjectTableDisplaySettings exibicao)
        {
            double headerWidth = MedirMaiorPalavra(
                column.NomeExibicao,
                new FontFamily(exibicao.FonteCabecalho),
                exibicao.TamanhoFonteCabecalho,
                exibicao.CabecalhoNegrito ? FontWeights.SemiBold : FontWeights.Normal);

            double bodyWidth = 0.0;

            foreach (ProjectTableDataRow row in rows)
            {
                if (columnIndex < 0 || columnIndex >= row.Cells.Count)
                    continue;

                bodyWidth = Math.Max(
                    bodyWidth,
                    MedirMaiorPalavra(
                        row.Cells[columnIndex].DisplayValue,
                        new FontFamily(exibicao.FonteCorpo),
                        exibicao.TamanhoFonteCorpo,
                        FontWeights.Normal));
            }

            double textWidth = Math.Max(headerWidth, bodyWidth);
            return Math.Max(MinimumColumnWidth, textWidth + HorizontalTextPadding + ColumnSafetyPadding);
        }

        private static double MedirMaiorPalavra(
            string? texto,
            FontFamily fontFamily,
            double fontSize,
            FontWeight fontWeight)
        {
            double maior = 0.0;

            foreach (string palavra in ExtrairPalavras(texto))
                maior = Math.Max(maior, MedirTexto(palavra, fontFamily, fontSize, fontWeight));

            return maior;
        }

        private static IEnumerable<string> ExtrairPalavras(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return Enumerable.Empty<string>();

            return texto
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p));
        }

        private static double MedirTexto(
            string texto,
            FontFamily fontFamily,
            double fontSize,
            FontWeight fontWeight)
        {
            try
            {
                var typeface = new Typeface(fontFamily, FontStyles.Normal, fontWeight, FontStretches.Normal);
                var formattedText = new FormattedText(
                    texto,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    Brushes.Black,
                    1.0);

                return formattedText.WidthIncludingTrailingWhitespace;
            }
            catch
            {
                return texto.Length * Math.Max(1.0, fontSize) * 0.65;
            }
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

    public sealed class ProjectSheetTableColumnViewModel
    {
        public ProjectSheetTableColumnViewModel(ProjectTableDataColumn column, double width)
        {
            Categoria = column.Categoria;
            CampoId = column.CampoId;
            NomeExibicao = column.NomeExibicao;
            Ordem = column.Ordem;
            Width = width;
        }

        public ProjectTableElementCategory Categoria { get; }
        public string CampoId { get; }
        public string NomeExibicao { get; }
        public int Ordem { get; }
        public double Width { get; }
    }

    public sealed class ProjectSheetTableRowViewModel
    {
        private const double MinimumCellWidth = 44.0;

        public ProjectSheetTableRowViewModel(
            ProjectTableDataRow row,
            int visualIndex,
            ProjectTableDisplaySettings exibicao,
            IReadOnlyList<ProjectSheetTableColumnViewModel> columns)
        {
            ElementoId = row.ElementoId;
            Cells = row.Cells
                .Select((cell, index) => new ProjectSheetTableCellViewModel(
                    cell.DisplayValue,
                    index >= 0 && index < columns.Count ? columns[index].Width : MinimumCellWidth))
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
        public ProjectSheetTableCellViewModel(string displayValue, double width)
        {
            DisplayValue = displayValue ?? string.Empty;
            Width = width;
        }

        public string DisplayValue { get; }
        public double Width { get; }
    }
}