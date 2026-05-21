using System.Collections;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class CargaViewModel : ElementoViewModel
    {
        public CargaViewModel(
            Carga modelo,
            TypeLibraryService types)
            : base(
                modelo,
                new EquipamentoNode(modelo),
                types)
        {
            SelecionarPrimeiroTipoDisponivel();
            AtualizarTerminais();
        }

        public Carga Carga => (Carga)Modelo;

        public override IEnumerable TiposDisponiveis =>
            Types.TiposCargas;

        protected override void AtualizarNode()
        {
            base.AtualizarNode();
            AtualizarTerminais();
        }

        private void AtualizarTerminais()
        {
            Carga.AtualizarTerminais(Largura);
        }

        public double PotenciaAtivaKW
        {
            get => Carga.PotenciaAtivaKW;
            set
            {
                if (Carga.PotenciaAtivaKW == value)
                    return;

                Carga.PotenciaAtivaKW = value;
                OnPropertyChanged();
            }
        }

        public double PotenciaReativaKvar
        {
            get => Carga.PotenciaReativaKvar;
            set
            {
                if (Carga.PotenciaReativaKvar == value)
                    return;

                Carga.PotenciaReativaKvar = value;
                OnPropertyChanged();
            }
        }

        public string Nome
        {
            get => Carga.Nome;
            set
            {
                if (Carga.Nome == value)
                    return;

                Carga.Nome = value;
                OnPropertyChanged();
            }
        }

        public string Barra
        {
            get => Carga.Barra;
            set
            {
                if (Carga.Barra == value)
                    return;

                Carga.Barra = value;
                OnPropertyChanged();
            }
        }

        public string Alimentador
        {
            get => Carga.Alimentador;
            set
            {
                if (Carga.Alimentador == value)
                    return;

                Carga.Alimentador = value;
                OnPropertyChanged();
            }
        }
    }
}