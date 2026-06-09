using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetViewModel : INotifyPropertyChanged
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheet _sheet;
        private readonly MoverTabelaNaPranchaUseCase? _moverTabelaNaPrancha;
        private readonly RemoverTabelaDaPranchaUseCase? _removerTabelaDaPrancha;
        private string _titulo = string.Empty;
        private string _emptyMessage = string.Empty;
        private Guid? _selectedInstanceId;

        public ProjectSheetViewModel(
            AraciDocument document,
            ProjectSheet sheet,
            MoverTabelaNaPranchaUseCase? moverTabelaNaPrancha = null,
            RemoverTabelaDaPranchaUseCase? removerTabelaDaPrancha = null)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _moverTabelaNaPrancha = moverTabelaNaPrancha;
            _removerTabelaDaPrancha = removerTabelaDaPrancha;
            TableInstances = new ObservableCollection<ProjectSheetTableInstanceViewModel>();
            Refresh();
        }

        public Guid SheetId => _sheet.Id;
        public ObservableCollection<ProjectSheetTableInstanceViewModel> TableInstances { get; }
        public bool HasInstances => TableInstances.Count > 0;
        public bool HasEmptyMessage => !string.IsNullOrWhiteSpace(EmptyMessage);
        public Guid? SelectedInstanceId => _selectedInstanceId;
        public bool HasSelectedInstance => _selectedInstanceId.HasValue;

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
            Guid? selectedBeforeRefresh = _selectedInstanceId;
            Titulo = string.IsNullOrWhiteSpace(_sheet.Numero)
                ? _sheet.Nome
                : $"{_sheet.Numero} - {_sheet.Nome}";

            TableInstances.Clear();

            foreach (ProjectSheetTableInstance instance in _sheet.Tabelas.Where(i => i != null))
            {
                ProjectTable? table = _document.Tabelas.FirstOrDefault(t => t.Id == instance.TableId);
                var instanceViewModel = new ProjectSheetTableInstanceViewModel(instance, table?.Nome ?? "Tabela nao encontrada")
                {
                    IsSelected = selectedBeforeRefresh == instance.Id
                };
                TableInstances.Add(instanceViewModel);
            }

            if (selectedBeforeRefresh.HasValue && !TableInstances.Any(i => i.Id == selectedBeforeRefresh.Value))
                _selectedInstanceId = null;

            EmptyMessage = HasInstances ? string.Empty : "Nenhuma tabela inserida na prancha";
            OnPropertyChanged(nameof(HasInstances));
            OnSelectionChanged();
        }

        public void SelecionarInstancia(Guid instanceId)
        {
            bool encontrou = false;

            foreach (ProjectSheetTableInstanceViewModel instance in TableInstances)
            {
                bool selecionada = instance.Id == instanceId;
                instance.IsSelected = selecionada;
                encontrou |= selecionada;
            }

            _selectedInstanceId = encontrou ? instanceId : null;
            OnSelectionChanged();
        }

        public void LimparSelecao()
        {
            foreach (ProjectSheetTableInstanceViewModel instance in TableInstances)
                instance.IsSelected = false;

            _selectedInstanceId = null;
            OnSelectionChanged();
        }

        public bool MoverInstancia(Guid instanceId, double novoX, double novoY)
        {
            ProjectSheetTableInstanceViewModel? instanceViewModel = TableInstances.FirstOrDefault(i => i.Id == instanceId);

            if (instanceViewModel == null)
                return false;

            bool moved = _moverTabelaNaPrancha?.Mover(SheetId, instanceId, novoX, novoY) == true;

            if (moved)
            {
                Refresh();
                SelecionarInstancia(instanceId);
            }

            return moved;
        }

        public bool RemoverInstanciaSelecionada()
        {
            if (!_selectedInstanceId.HasValue)
                return false;

            Guid instanceId = _selectedInstanceId.Value;
            bool removed = _removerTabelaDaPrancha?.Remover(SheetId, instanceId) == true;

            if (removed)
            {
                _selectedInstanceId = null;
                Refresh();
            }

            return removed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnSelectionChanged()
        {
            OnPropertyChanged(nameof(SelectedInstanceId));
            OnPropertyChanged(nameof(HasSelectedInstance));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}