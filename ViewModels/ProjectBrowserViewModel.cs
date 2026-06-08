using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Araci.Core.Documents;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public sealed class ProjectBrowserViewModel
    {
        private readonly AraciDocument _document;
        private Guid? _selectedItemId;
        private string? _selectedItemKind;

        public ProjectBrowserViewModel(AraciDocument document)
        {
            _document = document;
            Secoes = new ObservableCollection<ProjectBrowserSectionViewModel>
            {
                new("Vistas"),
                new("Tabelas"),
                new("Pranchas")
            };

            AtualizarSecoes();

            _document.Vistas.CollectionChanged += OnDocumentCollectionChanged;
            _document.Tabelas.CollectionChanged += OnDocumentCollectionChanged;
            _document.Pranchas.CollectionChanged += OnDocumentCollectionChanged;
            _document.VistaAtivaAlterada += OnVistaAtivaAlterada;
        }

        public ObservableCollection<ProjectBrowserSectionViewModel> Secoes { get; }

        private void OnDocumentCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            AtualizarSecoes();
        }

        private void OnVistaAtivaAlterada()
        {
            foreach (ProjectBrowserSectionViewModel secao in Secoes)
            {
                foreach (ProjectBrowserItemViewModel item in secao.Itens)
                    item.IsActiveView = item.Tipo == "Vista" && _document.VistaAtivaId == item.Id;
            }
        }

        private void AtualizarSecoes()
        {
            AtualizarItens(Secoes[0], _document.Vistas.Select(v => CriarItem(v.Id, "Vista", v.Nome)));
            AtualizarItens(Secoes[1], CriarItensTabela());
            AtualizarItens(Secoes[2], CriarItensPrancha());
        }

        private IEnumerable<ProjectBrowserItemViewModel> CriarItensTabela()
        {
            if (_document.Tabelas.Count == 0)
            {
                yield return ProjectBrowserItemViewModel.CriarPlaceholder("Nenhuma tabela");
                yield break;
            }

            foreach (ProjectTable tabela in _document.Tabelas)
                yield return CriarItem(tabela.Id, "Tabela", tabela.Nome);
        }

        private IEnumerable<ProjectBrowserItemViewModel> CriarItensPrancha()
        {
            if (_document.Pranchas.Count == 0)
            {
                yield return ProjectBrowserItemViewModel.CriarPlaceholder("Nenhuma prancha");
                yield break;
            }

            foreach (ProjectSheet prancha in _document.Pranchas)
                yield return CriarItem(prancha.Id, "Prancha", FormatarPrancha(prancha));
        }

        private ProjectBrowserItemViewModel CriarItem(Guid id, string tipo, string nome)
        {
            var item = new ProjectBrowserItemViewModel(
                id,
                tipo,
                nome,
                true,
                SelecionarItem)
            {
                IsActiveView = tipo == "Vista" && _document.VistaAtivaId == id
            };

            item.IsSelected = _selectedItemId == id && _selectedItemKind == tipo;
            return item;
        }

        private void SelecionarItem(ProjectBrowserItemViewModel item)
        {
            if (!item.IsSelectable)
                return;

            _selectedItemId = item.Id;
            _selectedItemKind = item.Tipo;

            if (item.Tipo == "Vista")
                _document.DefinirVistaAtiva(item.Id);

            foreach (ProjectBrowserSectionViewModel secao in Secoes)
            {
                foreach (ProjectBrowserItemViewModel atual in secao.Itens)
                {
                    atual.IsSelected = ReferenceEquals(atual, item);
                    atual.IsActiveView = atual.Tipo == "Vista" && _document.VistaAtivaId == atual.Id;
                }
            }
        }

        private static void AtualizarItens(ProjectBrowserSectionViewModel secao, IEnumerable<ProjectBrowserItemViewModel> itens)
        {
            secao.Itens.Clear();

            foreach (ProjectBrowserItemViewModel item in itens)
                secao.Itens.Add(item);
        }

        private static string FormatarPrancha(ProjectSheet prancha)
        {
            return string.IsNullOrWhiteSpace(prancha.Numero)
                ? prancha.Nome
                : $"{prancha.Numero} - {prancha.Nome}";
        }
    }

    public sealed class ProjectBrowserSectionViewModel
    {
        public ProjectBrowserSectionViewModel(string titulo)
        {
            Titulo = titulo;
            Itens = new ObservableCollection<ProjectBrowserItemViewModel>();
        }

        public string Titulo { get; }
        public ObservableCollection<ProjectBrowserItemViewModel> Itens { get; }
    }

    public sealed class ProjectBrowserItemViewModel : INotifyPropertyChanged
    {
        private readonly Action<ProjectBrowserItemViewModel>? _selecionar;
        private bool _isSelected;
        private bool _isActiveView;

        public ProjectBrowserItemViewModel(Guid id, string tipo, string nome, bool isSelectable, Action<ProjectBrowserItemViewModel>? selecionar)
        {
            Id = id;
            Tipo = tipo;
            Nome = nome;
            IsSelectable = isSelectable;
            _selecionar = selecionar;
            SelecionarCommand = new RelayCommand(Selecionar, () => IsSelectable);
        }

        public Guid Id { get; }
        public string Tipo { get; }
        public string Nome { get; }
        public bool IsSelectable { get; }
        public ICommand SelecionarCommand { get; }
        public bool IsActiveView
        {
            get => _isActiveView;
            set
            {
                if (_isActiveView == value)
                    return;

                _isActiveView = value;
                OnPropertyChanged();
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

        public static ProjectBrowserItemViewModel CriarPlaceholder(string nome)
        {
            return new ProjectBrowserItemViewModel(Guid.Empty, "Placeholder", nome, false, null);
        }

        private void Selecionar()
        {
            _selecionar?.Invoke(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
