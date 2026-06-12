using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly AraciDocument _document;
        private readonly ProjectTable _table;
        private readonly ProjectTableDataBuilder _builder;
        private string _titulo = string.Empty;
        private IReadOnlyList<ProjectTableDataColumn> _columns = new List<ProjectTableDataColumn>();
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
        public FontWeight TitleFontWeight => Exibicao.TituloNegrito ? FontWeights.SemiBold : FontWeights.Normal;
        public FontWeight HeaderFontWeight => Exibicao.CabecalhoNegrito ? FontWeights.SemiBold : FontWeights.Normal;
        public Brush TitleForegroundBrush => CriarBrush(Exibicao.CorTextoTitulo, ProjectTableDisplaySettings.DefaultTitleTextColor);
        public Brush TitleBackgroundBrush => CriarBrush(Exibicao.CorFundoTitulo, ProjectTableDisplaySettings.DefaultTitleBackgroundColor);
        public Brush HeaderForegroundBrush => CriarBrush(Exibicao.CorTextoCabecalho, ProjectTableDisplaySettings.DefaultHeaderTextColor);
        public Brush HeaderBackgroundBrush => CriarBrush(Exibicao.CorFundoCabecalho, ProjectTableDisplaySettings.DefaultHeaderBackgroundColor);
        public Brush BodyForegroundBrush => CriarBrush(Exibicao.CorTextoCorpo, ProjectTableDisplaySettings.DefaultBodyTextColor);
        public Brush BodyBackgroundBrush => CriarBrush(Exibicao.CorFundoCorpo, ProjectTableDisplaySettings.DefaultBodyBackgroundColor);
        public Brush GridBrush => CriarBrush(Exibicao.CorGrade, ProjectTableDisplaySettings.DefaultGridColor);
        public Thickness GridBorderThickness => Exibicao.ExibirLinhasGrade ? new Thickness(0, 0, Exibicao.EspessuraGrade, Exibicao.EspessuraGrade) : new Thickness(0);
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

        public void Refresh()
        {
            Titulo = _table.Nome;
            ProjectTableDataResult result = _builder.Build(_document, _table);
            Columns = result.Columns.ToList();

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
            OnPropertyChanged(nameof(TitleFontWeight));
            OnPropertyChanged(nameof(HeaderFontWeight));
            OnPropertyChanged(nameof(TitleForegroundBrush));
            OnPropertyChanged(nameof(TitleBackgroundBrush));
            OnPropertyChanged(nameof(HeaderForegroundBrush));
            OnPropertyChanged(nameof(HeaderBackgroundBrush));
            OnPropertyChanged(nameof(BodyForegroundBrush));
            OnPropertyChanged(nameof(BodyBackgroundBrush));
            OnPropertyChanged(nameof(GridBrush));
            OnPropertyChanged(nameof(GridBorderThickness));
            OnPropertyChanged(nameof(TitleTextAlignment));
            OnPropertyChanged(nameof(HeaderTextAlignment));
            OnPropertyChanged(nameof(BodyTextAlignment));
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