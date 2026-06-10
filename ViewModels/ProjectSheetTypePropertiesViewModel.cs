using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTypePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly RenomearItemProjetoUseCase _renomearItemProjeto;
        private readonly EditarPropriedadesTipoPranchaUseCase _editarPropriedadesTipoPrancha;

        public ProjectSheetTypePropertiesViewModel(
            AraciDocument document,
            ProjectSheetType tipo,
            RenomearItemProjetoUseCase renomearItemProjeto,
            EditarPropriedadesTipoPranchaUseCase editarPropriedadesTipoPrancha)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _renomearItemProjeto = renomearItemProjeto ?? throw new ArgumentNullException(nameof(renomearItemProjeto));
            _editarPropriedadesTipoPrancha = editarPropriedadesTipoPrancha ?? throw new ArgumentNullException(nameof(editarPropriedadesTipoPrancha));
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _document.PropriedadesTipoPranchaAlteradas += OnPropriedadesTipoPranchaAlteradas;
        }

        public Guid Id => _tipo.Id;
        public string Titulo => "Tipo de Prancha";

        public IReadOnlyList<ProjectSheetFormat> Formatos { get; } =
            new[]
            {
                ProjectSheetFormat.A4,
                ProjectSheetFormat.A3,
                ProjectSheetFormat.A2,
                ProjectSheetFormat.A1,
                ProjectSheetFormat.A0,
                ProjectSheetFormat.Personalizado
            };

        public IReadOnlyList<ProjectSheetOrientation> Orientacoes { get; } =
            new[]
            {
                ProjectSheetOrientation.Paisagem,
                ProjectSheetOrientation.Retrato
            };

        public string Nome
        {
            get => _tipo.Nome;
            set
            {
                string anterior = _tipo.Nome;
                bool renomeado = _renomearItemProjeto.RenomearTipoPrancha(_tipo.Id, value);

                if (!renomeado || anterior != _tipo.Nome)
                    OnPropertyChanged();
            }
        }

        public ProjectSheetFormat FormatoFolha
        {
            get => _tipo.FormatoFolha;
            set
            {
                if (_editarPropriedadesTipoPrancha.AlterarFormato(_tipo.Id, value))
                    OnPagePropertiesChanged();
            }
        }

        public ProjectSheetOrientation OrientacaoFolha
        {
            get => _tipo.OrientacaoFolha;
            set
            {
                if (_editarPropriedadesTipoPrancha.AlterarOrientacao(_tipo.Id, value))
                    OnPagePropertiesChanged();
            }
        }

        public double LarguraFolha
        {
            get => _tipo.LarguraFolha;
            set
            {
                if (_editarPropriedadesTipoPrancha.AlterarLargura(_tipo.Id, value))
                    OnPagePropertiesChanged();
            }
        }

        public double AlturaFolha
        {
            get => _tipo.AlturaFolha;
            set
            {
                if (_editarPropriedadesTipoPrancha.AlterarAltura(_tipo.Id, value))
                    OnPagePropertiesChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnItemProjetoRenomeado()
        {
            OnPropertyChanged(nameof(Nome));
        }

        private void OnPropriedadesTipoPranchaAlteradas(ProjectSheetType tipo)
        {
            if (tipo.Id != _tipo.Id)
                return;

            OnPagePropertiesChanged();
        }

        private void OnPagePropertiesChanged()
        {
            OnPropertyChanged(nameof(FormatoFolha));
            OnPropertyChanged(nameof(OrientacaoFolha));
            OnPropertyChanged(nameof(LarguraFolha));
            OnPropertyChanged(nameof(AlturaFolha));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
