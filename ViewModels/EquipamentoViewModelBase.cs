using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;
using Araci.Services.UI;
using Araci.Services.Catalog;

namespace Araci.ViewModels
{
    public abstract class EquipamentoViewModelBase<T> : ElementoViewModel
        where T : ElementoEquipamento
    {
        protected readonly T _equipamento;

        protected EquipamentoViewModelBase(
            T equipamento,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(equipamento, new EquipamentoNode(equipamento), types, names, typePropertiesDialogs)
        {
            _equipamento = equipamento;
        }

        public string Nome
        {
            get => _equipamento.Nome;
            set
            {
                if (_equipamento.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public int Alimentador
        {
            get => _equipamento.Alimentador;
            set
            {
                if (_equipamento.Alimentador == value)
                    return;

                _equipamento.Alimentador = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaAtiva
        {
            get => _equipamento.PotenciaAtiva;
            set
            {
                if (_equipamento.PotenciaAtiva == value)
                    return;

                _equipamento.PotenciaAtiva = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }
    }
}
