using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetViewModel : INotifyPropertyChanged
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheet _sheet;
        private string _titulo = string.Empty;
        private string _emptyMessage = string.Empty;

        public ProjectSheetViewModel(AraciDocument document, ProjectSheet sheet)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            TableInstances = new ObservableCollection<ProjectSheetTableInstanceViewModel>();
            Refresh();
        }

        public Guid SheetId => _sheet.Id;
        public ObservableCollection<ProjectSheetTableInstanceViewModel> TableInstances { get; }
        public bool HasInstances => TableInstances.Count > 0;
        public bool HasEmptyMessage => !string.IsNullOrWhiteSpace(EmptyMessage);

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
            Titulo = string.IsNullOrWhiteSpace(_sheet.Numero)
                ? _sheet.Nome
                : $"{_sheet.Numero} - {_sheet.Nome}";

            TableInstances.Clear();

            foreach (ProjectSheetTableInstance instance in _sheet.Tabelas.Where(i => i != null))
            {
                ProjectTable? table = _document.Tabelas.FirstOrDefault(t => t.Id == instance.TableId);
                TableInstances.Add(new ProjectSheetTableInstanceViewModel(instance, table?.Nome ?? "Tabela nao encontrada"));
            }

            EmptyMessage = HasInstances ? string.Empty : "Nenhuma tabela inserida na prancha";
            OnPropertyChanged(nameof(HasInstances));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
