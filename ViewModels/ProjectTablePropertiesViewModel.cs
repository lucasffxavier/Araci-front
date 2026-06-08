using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly IUserDialogService _dialogs;

        public ProjectTablePropertiesViewModel(
            AraciDocument document,
            ProjectTable tabela,
            RenomearItemProjetoUseCase renomearItemProjeto,
            EditarPropriedadesTabelaUseCase editarPropriedadesTabela,
            IUserDialogService dialogs)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
            _renomearItemProjeto = renomearItemProjeto ?? throw new ArgumentNullException(nameof(renomearItemProjeto));
            _editarPropriedadesTabela = editarPropriedadesTabela ?? throw new ArgumentNullException(nameof(editarPropriedadesTabela));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _document.PropriedadesTabelaAlteradas += OnPropriedadesTabelaAlteradas;

            ExportarCsvCommand = new RelayCommand(MostrarExportarCsvPlaceholder);
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

        private void MostrarExportarCsvPlaceholder()
        {
            _dialogs.ShowInfo(
                "Exportar CSV",
                "A exportação CSV será implementada em uma etapa futura.");
        }

        private void MostrarElementosTabelaPlaceholder()
        {
            _dialogs.ShowElementosTabelaPlaceholder();
        }

        private void MostrarFiltrosPlaceholder()
        {
            _dialogs.ShowInfo(
                "Filtros",
                "A configuração de filtros da tabela será implementada em uma etapa futura.");
        }

        private void MostrarOrdenacaoPlaceholder()
        {
            _dialogs.ShowInfo(
                "Ordenação",
                "A configuração de ordenação da tabela será implementada em uma etapa futura.");
        }

        private void MostrarExibicaoPlaceholder()
        {
            _dialogs.ShowInfo(
                "Exibição",
                "A configuração de exibição da tabela será implementada em uma etapa futura.");
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
