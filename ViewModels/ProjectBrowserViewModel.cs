using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public sealed class ProjectBrowserViewModel
    {
        private readonly AraciDocument _document;
        private readonly Action<Guid> _definirVistaAtiva;
        private readonly RenomearItemProjetoUseCase? _renomearItemProjeto;
        private readonly ExcluirItemProjetoUseCase? _excluirItemProjeto;
        private readonly DuplicarItemProjetoUseCase? _duplicarItemProjeto;
        private readonly Action<Guid>? _abrirPropriedadesVista;
        private readonly Action<Guid>? _abrirPropriedadesTabela;
        private Guid? _selectedItemId;
        private string? _selectedItemKind;

        public ProjectBrowserViewModel(
            AraciDocument document,
            Action<Guid>? definirVistaAtiva = null,
            RenomearItemProjetoUseCase? renomearItemProjeto = null,
            ExcluirItemProjetoUseCase? excluirItemProjeto = null,
            DuplicarItemProjetoUseCase? duplicarItemProjeto = null,
            Action<Guid>? abrirPropriedadesVista = null,
            Action<Guid>? abrirPropriedadesTabela = null)
        {
            _document = document;
            _definirVistaAtiva = definirVistaAtiva ?? _document.DefinirVistaAtiva;
            _renomearItemProjeto = renomearItemProjeto;
            _excluirItemProjeto = excluirItemProjeto;
            _duplicarItemProjeto = duplicarItemProjeto;
            _abrirPropriedadesVista = abrirPropriedadesVista;
            _abrirPropriedadesTabela = abrirPropriedadesTabela;
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
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            ExcluirSelecionadoCommand = new RelayCommand(ExecutarExcluirSelecionado);
        }

        public ObservableCollection<ProjectBrowserSectionViewModel> Secoes { get; }
        public ICommand ExcluirSelecionadoCommand { get; }

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

        private void OnItemProjetoRenomeado()
        {
            AtualizarSecoes();
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
                yield return CriarItem(prancha.Id, "Prancha", FormatarPrancha(prancha), prancha.Nome);
        }

        private ProjectBrowserItemViewModel CriarItem(Guid id, string tipo, string nome, string? nomeEdicao = null)
        {
            var item = new ProjectBrowserItemViewModel(
                id,
                tipo,
                nome,
                nomeEdicao ?? nome,
                true,
                SelecionarItem,
                RenomearItem,
                ExcluirItem,
                DuplicarItem,
                AbrirPropriedadesItem)
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
            {
                _definirVistaAtiva(item.Id);
                _abrirPropriedadesVista?.Invoke(item.Id);
            }
            else if (item.Tipo == "Tabela")
            {
                _abrirPropriedadesTabela?.Invoke(item.Id);
            }

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

        private bool RenomearItem(ProjectBrowserItemViewModel item, string novoNome)
        {
            if (_renomearItemProjeto == null)
                return false;

            switch (item.Tipo)
            {
                case "Vista":
                    return _renomearItemProjeto.RenomearVista(item.Id, novoNome);

                case "Tabela":
                    return _renomearItemProjeto.RenomearTabela(item.Id, novoNome);

                case "Prancha":
                    return _renomearItemProjeto.RenomearPrancha(item.Id, novoNome);

                default:
                    return false;
            }
        }

        private bool ExcluirItem(ProjectBrowserItemViewModel item)
        {
            if (_excluirItemProjeto == null)
                return false;

            return item.Tipo switch
            {
                "Vista" => _excluirItemProjeto.ExcluirVista(item.Id),
                "Tabela" => _excluirItemProjeto.ExcluirTabela(item.Id),
                "Prancha" => _excluirItemProjeto.ExcluirPrancha(item.Id),
                _ => false
            };
        }

        private bool DuplicarItem(ProjectBrowserItemViewModel item)
        {
            if (_duplicarItemProjeto == null)
                return false;

            return item.Tipo switch
            {
                "Vista" => _duplicarItemProjeto.DuplicarVista(item.Id),
                "Tabela" => _duplicarItemProjeto.DuplicarTabela(item.Id),
                "Prancha" => _duplicarItemProjeto.DuplicarPrancha(item.Id),
                _ => false
            };
        }

        private void AbrirPropriedadesItem(ProjectBrowserItemViewModel item)
        {
            if (item.Tipo == "Vista")
                _abrirPropriedadesVista?.Invoke(item.Id);
            else if (item.Tipo == "Tabela")
                _abrirPropriedadesTabela?.Invoke(item.Id);
        }

        public bool ExcluirSelecionado()
        {
            ProjectBrowserItemViewModel? item = Secoes
                .SelectMany(secao => secao.Itens)
                .FirstOrDefault(atual =>
                    atual.IsSelected &&
                    atual.Id == _selectedItemId &&
                    atual.Tipo == _selectedItemKind);

            return item?.TentarExcluir() == true;
        }

        private void ExecutarExcluirSelecionado()
        {
            ExcluirSelecionado();
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
        private readonly Func<ProjectBrowserItemViewModel, string, bool>? _renomear;
        private readonly Func<ProjectBrowserItemViewModel, bool>? _excluir;
        private readonly Func<ProjectBrowserItemViewModel, bool>? _duplicar;
        private readonly Action<ProjectBrowserItemViewModel>? _abrirPropriedades;
        private bool _isSelected;
        private bool _isActiveView;
        private bool _isEditing;
        private string _nome;
        private string _nomeEdicao;
        private string _textoEdicao;

        public ProjectBrowserItemViewModel(
            Guid id,
            string tipo,
            string nome,
            string nomeEdicao,
            bool isSelectable,
            Action<ProjectBrowserItemViewModel>? selecionar,
            Func<ProjectBrowserItemViewModel, string, bool>? renomear = null,
            Func<ProjectBrowserItemViewModel, bool>? excluir = null,
            Func<ProjectBrowserItemViewModel, bool>? duplicar = null,
            Action<ProjectBrowserItemViewModel>? abrirPropriedades = null)
        {
            Id = id;
            Tipo = tipo;
            _nome = nome;
            _nomeEdicao = nomeEdicao;
            _textoEdicao = nomeEdicao;
            IsSelectable = isSelectable;
            _selecionar = selecionar;
            _renomear = renomear;
            _excluir = excluir;
            _duplicar = duplicar;
            _abrirPropriedades = abrirPropriedades;
            SelecionarCommand = new RelayCommand(Selecionar, () => IsSelectable);
            IniciarEdicaoCommand = new RelayCommand(IniciarEdicao, () => IsSelectable);
            ConfirmarEdicaoCommand = new RelayCommand(ConfirmarEdicao);
            CancelarEdicaoCommand = new RelayCommand(CancelarEdicao);
            ExcluirCommand = new RelayCommand(Excluir, () => IsSelectable && _excluir != null);
            DuplicarCommand = new RelayCommand(Duplicar, () => IsSelectable && _duplicar != null);
            PropriedadesCommand = new RelayCommand(AbrirPropriedades, () => IsSelectable && (Tipo == "Vista" || Tipo == "Tabela") && _abrirPropriedades != null);
        }

        public Guid Id { get; }
        public string Tipo { get; }
        public string Nome
        {
            get => _nome;
            private set
            {
                if (_nome == value)
                    return;

                _nome = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelectable { get; }
        public ICommand SelecionarCommand { get; }
        public ICommand IniciarEdicaoCommand { get; }
        public ICommand ConfirmarEdicaoCommand { get; }
        public ICommand CancelarEdicaoCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand DuplicarCommand { get; }
        public ICommand PropriedadesCommand { get; }

        public string TextoEdicao
        {
            get => _textoEdicao;
            set
            {
                if (_textoEdicao == value)
                    return;

                _textoEdicao = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing == value)
                    return;

                _isEditing = value;
                OnPropertyChanged();
            }
        }

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
            return new ProjectBrowserItemViewModel(Guid.Empty, "Placeholder", nome, nome, false, null);
        }

        public void AtualizarNomeExibicao(string nome, string? nomeEdicao = null)
        {
            Nome = nome;
            _nomeEdicao = nomeEdicao ?? nome;
            TextoEdicao = _nomeEdicao;
        }

        private void Selecionar()
        {
            _selecionar?.Invoke(this);
        }

        private void IniciarEdicao()
        {
            if (!IsSelectable)
                return;

            TextoEdicao = _nomeEdicao;
            IsEditing = true;
        }

        private void ConfirmarEdicao()
        {
            if (!IsEditing)
                return;

            string nomeAnterior = _nomeEdicao;

            if (_renomear?.Invoke(this, TextoEdicao) != true)
                TextoEdicao = nomeAnterior;

            IsEditing = false;
        }

        private void CancelarEdicao()
        {
            TextoEdicao = _nomeEdicao;
            IsEditing = false;
        }

        public bool TentarExcluir()
        {
            return _excluir?.Invoke(this) == true;
        }

        private void Excluir()
        {
            TentarExcluir();
        }

        private void Duplicar()
        {
            _duplicar?.Invoke(this);
        }

        private void AbrirPropriedades()
        {
            _abrirPropriedades?.Invoke(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
