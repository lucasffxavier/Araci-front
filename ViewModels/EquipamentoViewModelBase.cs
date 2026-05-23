using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public abstract class EquipamentoViewModelBase<T> : ElementoViewModel
        where T : ElementoEquipamento
    {
        protected readonly T _equipamento;

        protected EquipamentoViewModelBase(T equipamento, TypeLibraryService types)
            : base(equipamento, new EquipamentoNode(equipamento), types)
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

                _equipamento.Nome = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Alimentador
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
