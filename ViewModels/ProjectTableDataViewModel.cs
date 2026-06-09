using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
