using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed class ProjectTableDataViewModel : INotifyPropertyChanged
    {
        private const double MinimumColumnWidth = 44.0;
        private const double HeaderHorizontalPadding = 14.0;
        private const double BodyHorizontalPadding = 14.0;

        private readonly AraciDocument _document;
        private readonly ProjectTable _table;
        private readonly ProjectTableDataBuilder _builder;
        private string _titulo = string.Empty;
        private IReadOnlyList<ProjectTableDataColumn> _columns = new List<ProjectTableDataColumn>();
        private IReadOnlyList<double> _columnMinimumWidths = new List<double>();
        private string _emptyMessage = string.Empty;

        public ProjectTableDataViewModel(
            AraciDocument document,
            ProjectTable table,
            ProjectTableDataBuilder? builder = null)
        {
            _document = document;
            _table = table;
            _builder = builder ?? new ProjectTableDataBuilder();
            Rows = new ObservableCollection<ProjectTableDataRowViewModel>();
            Refresh();
        }

        public Guid TableId => _table.Id;
        public ObservableCollection<ProjectTableDataRowViewModel> Rows { get; }
        public ProjectTableDisplaySettings Exibicao => _table.Exibicao ?? new ProjectTableDisplaySettings();
        public FontFamily TitleFontFamily => new(Exibicao.FonteTitulo);
        public FontFamily HeaderFontFamily => new(Exibicao.FonteCabecalho);
        public FontFamily BodyFontFamily => new(Exibicao.FonteCorpo);
        public double TitleFontSize => Exibicao.TamanhoFonteTitulo;
        public double HeaderFontSize => Exibicao.TamanhoFonteCabecalho;
        public double BodyFontSize => Exibicao.TamanhoFonteCorpo;
        public double TitleHeight => NormalizarAltura(Exibicao.AlturaTitulo, 32.0);
        public double HeaderRowHeight => NormalizarAltura(Exibicao.AlturaCabecalho, 26.0);
        public double BodyRowHeight => NormalizarAltura(Exibicao.AlturaLinhaCorpo, 24.0);
        public FontWeight TitleFontWeight => Exibicao.TituloNegrito ? FontWeights.SemiBold : FontWeights.Normal;
        public FontWeight HeaderFontWeight => Exibicao.CabecalhoNegrito ? FontWeights.SemiBold : FontWeights.Normal;
        public Brush TitleForegroundBrush => CriarBrush(Exibicao.CorTextoTitulo, ProjectTableDisplaySettings.DefaultTitleTextColor);
        public Brush TitleBackgroundBrush => CriarBrush(Exibicao.CorFundoTitulo, ProjectTableDisplaySettings.DefaultTitleBackgroundColor);
        public Brush HeaderForegroundBrush => CriarBrush(Exibicao.CorTextoCabecalho, ProjectTableDisplaySettings.DefaultHeaderTextColor);
        public Brush HeaderBackgroundBrush => CriarBrush(Exibicao.CorFundoCabecalho, ProjectTableDisplaySettings.DefaultHeaderBackgroundColor);
        public Brush BodyForegroundBrush => CriarBrush(Exibicao.CorTextoCorpo, ProjectTableDisplaySettings.DefaultBodyTextColor);
        public Brush BodyBackgroundBrush => CriarBrush(Exibicao.CorFundoCorpo, ProjectTableDisplaySettings.DefaultBodyBackgroundColor);
        public Brush AlternateRowBackgroundBrush => Exibicao.UsarLinhasAlternadas
            ? CriarBrush(Exibicao.CorLinhaAlternada, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor)
            : BodyBackgroundBrush;
        public Brush GridBrush => CriarBrush(Exibicao.CorGrade, ProjectTableDisplaySettings.DefaultGridColor);
        public Brush OutlineBrush => CriarBrush(Exibicao.CorContorno, ProjectTableDisplaySettings.DefaultOutlineColor);
        public Thickness GridBorderThickness => Exibicao.ExibirLinhasGrade ? new Thickness(0, 0, Exibicao.EspessuraGrade, Exibicao.EspessuraGrade) : new Thickness(0);
        public Thickness GridSeparatorBorderThickness => Exibicao.ExibirLinhasGrade ? new Thickness(0, 0, 0, Exibicao.EspessuraGrade) : new Thickness(0);
        public Thickness OutlineBorderThickness => Exibicao.ExibirContornoExterno ? new Thickness(Exibicao.EspessuraContorno) : new Thickness(0);
        public TextAlignment TitleTextAlignment => ConverterAlinhamento(Exibicao.AlinhamentoTitulo);
        public TextAlignment HeaderTextAlignment => ConverterAlinhamento(Exibicao.AlinhamentoCabecalho);
        public TextAlignment BodyTextAlignment => ConverterAlinhamento(Exibicao.AlinhamentoCorpo);

        public string Titulo
        {
            get => _titulo;
            private set
            {
                if (_titulo == value)
                    return;

                _titulo = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<ProjectTableDataColumn> Columns
        {
            get => _columns;
            private set
            {
                _columns = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasColumns));
            }
        }

        public IReadOnlyList<double> ColumnMinimumWidths
        {
            get => _columnMinimumWidths;
            private set
            {
                _columnMinimumWidths = value ?? new List<double>();
                OnPropertyChanged();
            }
        }

        public bool HasColumns => Columns.Count > 0;
        public bool HasRows => Rows.Count > 0;
        public bool HasEmptyMessage => !string.IsNullOrWhiteSpace(EmptyMessage);

        public string EmptyMessage
        {
            get => _emptyMessage;
            private set
            {
                if (_emptyMessage == value)
                    return;

                _emptyMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEmptyMessage));
            }
        }

        public double ObterLarguraMinimaColuna(int index)
        {
            return index >= 0 && index < ColumnMinimumWidths.Count
                ? ColumnMinimumWidths[index]
                : MinimumColumnWidth;
        }

        public void Refresh()
        {
            Titulo = _table.Nome;
            ProjectTableDataResult result = _builder.Build(_document, _table);
            Columns = result.Columns.ToList();
            ColumnMinimumWidths = CalcularLargurasMinimas(result.Columns, result.Rows, Exibicao);

            Rows.Clear();

            foreach (ProjectTableDataRow row in result.Rows)
                Rows.Add(new ProjectTableDataRowViewModel(row));

            EmptyMessage = !HasColumns
                ? "Nenhum campo selecionado"
                : !HasRows
                    ? "Nenhum item encontrado"
                    : string.Empty;

            OnPropertyChanged(nameof(HasRows));
            OnPropertyChanged(nameof(Exibicao));
            OnPropertyChanged(nameof(TitleFontFamily));
            OnPropertyChanged(nameof(HeaderFontFamily));
            OnPropertyChanged(nameof(BodyFontFamily));
            OnPropertyChanged(nameof(TitleFontSize));
            OnPropertyChanged(nameof(HeaderFontSize));
            OnPropertyChanged(nameof(BodyFontSize));
            OnPropertyChanged(nameof(TitleHeight));
            OnPropertyChanged(nameof(HeaderRowHeight));
            OnPropertyChanged(nameof(BodyRowHeight));
            OnPropertyChanged(nameof(TitleFontWeight));
            OnPropertyChanged(nameof(HeaderFontWeight));
            OnPropertyChanged(nameof(TitleForegroundBrush));
            OnPropertyChanged(nameof(TitleBackgroundBrush));
            OnPropertyChanged(nameof(HeaderForegroundBrush));
            OnPropertyChanged(nameof(HeaderBackgroundBrush));
            OnPropertyChanged(nameof(BodyForegroundBrush));
            OnPropertyChanged(nameof(BodyBackgroundBrush));
            OnPropertyChanged(nameof(AlternateRowBackgroundBrush));
            OnPropertyChanged(nameof(GridBrush));
            OnPropertyChanged(nameof(OutlineBrush));
            OnPropertyChanged(nameof(GridBorderThickness));
            OnPropertyChanged(nameof(GridSeparatorBorderThickness));
            OnPropertyChanged(nameof(OutlineBorderThickness));
            OnPropertyChanged(nameof(TitleTextAlignment));
            OnPropertyChanged(nameof(HeaderTextAlignment));
            OnPropertyChanged(nameof(BodyTextAlignment));
        }

        private static IReadOnlyList<double> CalcularLargurasMinimas(
            IReadOnlyList<ProjectTableDataColumn> columns,
            IReadOnlyList<ProjectTableDataRow> rows,
            ProjectTableDisplaySettings exibicao)
        {
            var resultado = new List<double>();

            if (columns.Count == 0)
                return resultado;

            Typeface headerTypeface = CriarTypeface(exibicao.FonteCabecalho, exibicao.CabecalhoNegrito ? FontWeights.SemiBold : FontWeights.Normal);
            Typeface bodyTypeface = CriarTypeface(exibicao.FonteCorpo, FontWeights.Normal);
            double headerFontSize = NormalizarTamanhoFonte(exibicao.TamanhoFonteCabecalho, 10.0);
            double bodyFontSize = NormalizarTamanhoFonte(exibicao.TamanhoFonteCorpo, 10.5);

            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                double maiorLargura = MedirMaiorPalavra(columns[columnIndex].NomeExibicao, headerTypeface, headerFontSize) + HeaderHorizontalPadding;

                foreach (ProjectTableDataRow row in rows)
                {
                    if (columnIndex < 0 || columnIndex >= row.Cells.Count)
                        continue;

                    double larguraCelula = MedirMaiorPalavra(row.Cells[columnIndex].DisplayValue, bodyTypeface, bodyFontSize) + BodyHorizontalPadding;

                    if (larguraCelula > maiorLargura)
                        maiorLargura = larguraCelula;
                }

                resultado.Add(Math.Ceiling(Math.Max(MinimumColumnWidth, maiorLargura)));
            }

            return resultado;
        }

        private static Typeface CriarTypeface(string? fonte, FontWeight peso)
        {
            string nomeFonte = string.IsNullOrWhiteSpace(fonte)
                ? ProjectTableDisplaySettings.DefaultFontFamily
                : fonte.Trim();

            return new Typeface(new FontFamily(nomeFonte), FontStyles.Normal, peso, FontStretches.Normal);
        }

        private static double MedirMaiorPalavra(string? texto, Typeface typeface, double fontSize)
        {
            double maior = 0.0;

            foreach (string palavra in ObterPalavras(texto))
            {
                double largura = MedirTexto(palavra, typeface, fontSize);

                if (largura > maior)
                    maior = largura;
            }

            return maior;
        }

        private static IEnumerable<string> ObterPalavras(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                yield break;

            foreach (string palavra in texto.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                string normalizada = palavra.Trim();

                if (!string.IsNullOrWhiteSpace(normalizada))
                    yield return normalizada;
            }
        }

        private static double MedirTexto(string texto, Typeface typeface, double fontSize)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return 0.0;

            var formatted = new FormattedText(
                texto,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                1.0);

            return formatted.WidthIncludingTrailingWhitespace;
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

        private static double NormalizarAltura(double valor, double fallback)
        {
            double altura = double.IsNaN(valor) || double.IsInfinity(valor) ? fallback : valor;

            if (altura < ProjectTableDisplaySettings.MinRowHeight)
                return ProjectTableDisplaySettings.MinRowHeight;

            if (altura > ProjectTableDisplaySettings.MaxRowHeight)
                return ProjectTableDisplaySettings.MaxRowHeight;

            return altura;
        }

        private static double NormalizarTamanhoFonte(double valor, double fallback)
        {
            double tamanho = double.IsNaN(valor) || double.IsInfinity(valor) ? fallback : valor;

            if (tamanho < ProjectTableDisplaySettings.MinFontSize)
                return ProjectTableDisplaySettings.MinFontSize;

            if (tamanho > ProjectTableDisplaySettings.MaxFontSize)
                return ProjectTableDisplaySettings.MaxFontSize;

            return tamanho;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class ProjectTableDataRowViewModel
    {
        private readonly IReadOnlyList<ProjectTableDataCell> _cells;

        public ProjectTableDataRowViewModel(ProjectTableDataRow row)
        {
            ElementoId = row.ElementoId;
            ElementoNome = row.ElementoNome;
            Categoria = row.Categoria;
            _cells = row.Cells;
        }

        public Guid ElementoId { get; }
        public string ElementoNome { get; }
        public ProjectTableElementCategory Categoria { get; }

        public string this[int index] =>
            index >= 0 && index < _cells.Count
                ? _cells[index].DisplayValue
                : string.Empty;
    }
}