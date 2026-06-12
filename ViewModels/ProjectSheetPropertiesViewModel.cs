using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetPropertiesViewModel : INotifyPropertyChanged
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheet _prancha;
        private readonly RenomearItemProjetoUseCase _renomearItemProjeto;
        private readonly EditarPropriedadesPranchaUseCase _editarPropriedadesPrancha;

        public ProjectSheetPropertiesViewModel(
            AraciDocument document,
            ProjectSheet prancha,
            RenomearItemProjetoUseCase renomearItemProjeto,
            EditarPropriedadesPranchaUseCase editarPropriedadesPrancha)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _prancha = prancha ?? throw new ArgumentNullException(nameof(prancha));
            _renomearItemProjeto = renomearItemProjeto ?? throw new ArgumentNullException(nameof(renomearItemProjeto));
            _editarPropriedadesPrancha = editarPropriedadesPrancha ?? throw new ArgumentNullException(nameof(editarPropriedadesPrancha));
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _document.PropriedadesPranchaAlteradas += OnPropriedadesPranchaAlteradas;
            _document.PropriedadesTipoPranchaAlteradas += OnPropriedadesTipoPranchaAlteradas;
        }

        public string Titulo => "Prancha";
        public IReadOnlyList<ProjectSheetType> TiposPrancha => _document.TiposPrancha;
        public bool PossuiTiposPrancha => _document.TiposPrancha.Count > 0;

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

        public string Numero
        {
            get => _prancha.Numero;
            set
            {
                if (_editarPropriedadesPrancha.AlterarNumero(_prancha.Id, value))
                    OnPropertyChanged();
            }
        }

        public string Nome
        {
            get => _prancha.Nome;
            set
            {
                string anterior = _prancha.Nome;
                bool renomeado = _renomearItemProjeto.RenomearPrancha(_prancha.Id, value);

                if (!renomeado || anterior != _prancha.Nome)
                    OnPropertyChanged();
            }
        }

        public ProjectSheetType? TipoPrancha
        {
            get => ResolverTipoPrancha();
            set
            {
                if (value == null)
                    return;

                if (_editarPropriedadesPrancha.AlterarTipoPrancha(_prancha.Id, value.Id))
                    OnTipoPranchaChanged();
            }
        }

        public ProjectSheetFormat FormatoFolha
        {
            get => _prancha.FormatoFolha;
            set
            {
                if (_editarPropriedadesPrancha.AlterarFormato(_prancha.Id, value))
                    OnCompatibilidadeFolhaChanged();
            }
        }

        public ProjectSheetOrientation OrientacaoFolha
        {
            get => _prancha.OrientacaoFolha;
            set
            {
                if (_editarPropriedadesPrancha.AlterarOrientacao(_prancha.Id, value))
                    OnCompatibilidadeFolhaChanged();
            }
        }

        public double LarguraFolha
        {
            get => _prancha.LarguraFolha;
            set
            {
                if (_editarPropriedadesPrancha.AlterarLargura(_prancha.Id, value))
                    OnCompatibilidadeFolhaChanged();
            }
        }

        public double AlturaFolha
        {
            get => _prancha.AlturaFolha;
            set
            {
                if (_editarPropriedadesPrancha.AlterarAltura(_prancha.Id, value))
                    OnCompatibilidadeFolhaChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private ProjectSheetType? ResolverTipoPrancha()
        {
            return _document.ObterTipoPranchaDaPrancha(_prancha);
        }

        private void OnItemProjetoRenomeado()
        {
            OnPropertyChanged(nameof(Nome));
            OnTiposPranchaChanged();
        }

        private void OnPropriedadesPranchaAlteradas(ProjectSheet prancha)
        {
            if (prancha.Id != _prancha.Id)
                return;

            OnPropertyChanged(nameof(Numero));
            OnTipoPranchaChanged();
            OnCompatibilidadeFolhaChanged();
        }

        private void OnPropriedadesTipoPranchaAlteradas(ProjectSheetType tipo)
        {
            if (!_prancha.SheetTypeId.HasValue || tipo.Id != _prancha.SheetTypeId.Value)
                return;

            OnTipoPranchaChanged();
            OnCompatibilidadeFolhaChanged();
        }

        private void OnTipoPranchaChanged()
        {
            OnPropertyChanged(nameof(TipoPrancha));
            OnTiposPranchaChanged();
        }

        private void OnTiposPranchaChanged()
        {
            OnPropertyChanged(nameof(TiposPrancha));
            OnPropertyChanged(nameof(PossuiTiposPrancha));
        }

        private void OnCompatibilidadeFolhaChanged()
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