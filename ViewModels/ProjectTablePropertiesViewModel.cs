using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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

        public ProjectTablePropertiesViewModel(
            AraciDocument document,
            ProjectTable tabela,
            RenomearItemProjetoUseCase renomearItemProjeto,
            EditarPropriedadesTabelaUseCase editarPropriedadesTabela)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
            _renomearItemProjeto = renomearItemProjeto ?? throw new ArgumentNullException(nameof(renomearItemProjeto));
            _editarPropriedadesTabela = editarPropriedadesTabela ?? throw new ArgumentNullException(nameof(editarPropriedadesTabela));
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _document.PropriedadesTabelaAlteradas += OnPropriedadesTabelaAlteradas;

            ExportarCsvCommand = new RelayCommand(NoOp);
            ElementosTabelaCommand = new RelayCommand(NoOp);
            FiltrosCommand = new RelayCommand(NoOp);
            OrdenacaoCommand = new RelayCommand(NoOp);
            ExibicaoCommand = new RelayCommand(NoOp);
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

        private static void NoOp()
        {
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
