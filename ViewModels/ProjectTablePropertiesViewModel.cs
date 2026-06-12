using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Araci.Applications.Abstractions;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public sealed class ProjectTablePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly AraciDocument _document;
        private readonly ProjectTable _tabela;
        private readonly RenomearItemProjetoUseCase _renomearItemProjeto;
        private readonly EditarPropriedadesTabelaUseCase _editarPropriedadesTabela;
        private readonly ExportarTabelaUseCase _exportarTabela;
        private readonly IUserDialogService _dialogs;

        public ProjectTablePropertiesViewModel(
            AraciDocument document,
            ProjectTable tabela,
            RenomearItemProjetoUseCase renomearItemProjeto,
            EditarPropriedadesTabelaUseCase editarPropriedadesTabela,
            ExportarTabelaUseCase exportarTabela,
            IUserDialogService dialogs)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
            _renomearItemProjeto = renomearItemProjeto ?? throw new ArgumentNullException(nameof(renomearItemProjeto));
            _editarPropriedadesTabela = editarPropriedadesTabela ?? throw new ArgumentNullException(nameof(editarPropriedadesTabela));
            _exportarTabela = exportarTabela ?? throw new ArgumentNullException(nameof(exportarTabela));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _document.PropriedadesTabelaAlteradas += OnPropriedadesTabelaAlteradas;

            ExportarCsvCommand = new RelayCommand(ExportarCsv);
            ElementosTabelaCommand = new RelayCommand(MostrarElementosTabelaPlaceholder);
            FiltrosCommand = new RelayCommand(MostrarFiltrosPlaceholder);
            OrdenacaoCommand = new RelayCommand(MostrarOrdenacaoPlaceholder);
            ExibicaoCommand = new RelayCommand(MostrarExibicaoPlaceholder);
        }

        public string Titulo => "Tabela";

        public IReadOnlyList<ProjectViewDiscipline> Disciplinas { get; } =
            new[]
            {
                ProjectViewDiscipline.Coordenacao,
                ProjectViewDiscipline.Eletrica,
                ProjectViewDiscipline.Solar,
                ProjectViewDiscipline.Eolica,
                ProjectViewDiscipline.Distribuicao,
                ProjectViewDiscipline.Subestacao
            };

        public ICommand ExportarCsvCommand { get; }
        public ICommand ElementosTabelaCommand { get; }
        public ICommand FiltrosCommand { get; }
        public ICommand OrdenacaoCommand { get; }
        public ICommand ExibicaoCommand { get; }

        public string Nome
        {
            get => _tabela.Nome;
            set
            {
                string anterior = _tabela.Nome;
                bool renomeado = _renomearItemProjeto.RenomearTabela(_tabela.Id, value);

                if (!renomeado || anterior != _tabela.Nome)
                    OnPropertyChanged();
            }
        }

        public ProjectViewDiscipline Disciplina
        {
            get => _tabela.Disciplina;
            set
            {
                if (_editarPropriedadesTabela.AlterarDisciplina(_tabela.Id, value))
                    OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnItemProjetoRenomeado()
        {
            OnPropertyChanged(nameof(Nome));
        }

        private void OnPropriedadesTabelaAlteradas(ProjectTable tabela)
        {
            if (tabela.Id != _tabela.Id)
                return;

            OnPropertyChanged(nameof(Disciplina));
        }

        private void ExportarCsv()
        {
            _exportarTabela.Executar(_tabela);
        }

        private void MostrarElementosTabelaPlaceholder()
        {
            ElementosTabelaDialogResult? resultado =
                _dialogs.ShowElementosTabelaDialog(_tabela.CategoriasElementos, _tabela.CamposSelecionados);

            if (resultado != null)
                _editarPropriedadesTabela.AlterarElementosTabela(
                    _tabela.Id,
                    resultado.Categorias,
                    resultado.CamposSelecionados);
        }

        private void MostrarFiltrosPlaceholder()
        {
            FiltrosTabelaDialogResult? resultado =
                _dialogs.ShowFiltrosTabelaDialog(
                    _tabela.CamposSelecionados,
                    _document.Vistas.Select(v => new ProjectViewDialogOption(v.Id, v.Nome)).ToList(),
                    _tabela.FiltroVistaId,
                    _tabela.ModoFiltro,
                    _tabela.Filtros);

            if (resultado != null)
                _editarPropriedadesTabela.AlterarFiltrosTabela(
                    _tabela.Id,
                    resultado.FiltroVistaId,
                    resultado.Modo,
                    resultado.Filtros);
        }

        private void MostrarOrdenacaoPlaceholder()
        {
            OrdenacaoTabelaDialogResult? resultado =
                _dialogs.ShowOrdenacaoTabelaDialog(_tabela.CamposSelecionados, _tabela.Ordenacoes);

            if (resultado != null)
                _editarPropriedadesTabela.AlterarOrdenacaoTabela(_tabela.Id, resultado.Ordenacoes);
        }

        private void MostrarExibicaoPlaceholder()
        {
            ExibicaoTabelaDialogResult? resultado = _dialogs.ShowExibicaoTabelaDialog(_tabela.Exibicao);

            if (resultado != null)
                _editarPropriedadesTabela.AlterarExibicaoTabela(_tabela.Id, resultado.Exibicao);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}