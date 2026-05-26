using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class CargaViewModel : ElementoViewModel
    {
        public CargaViewModel(
            Carga modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new EquipamentoNode(modelo), types, names, typePropertiesDialogs)
        {
            SelecionarPrimeiroTipoDisponivel();
            AtualizarTerminais();
        }

        public Carga Carga => (Carga)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposCargas;

        public string Barra
        {
            get => Carga.Barra;
            set
            {
                if (Carga.Barra == value)
                    return;

                Carga.Barra = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string BarraId
        {
            get => Carga.BarraId;
            set
            {
                if (Carga.BarraId == value)
                    return;

                Carga.BarraId = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Nome
        {
            get => Carga.Nome;
            set
            {
                if (Carga.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaAtiva
        {
            get => Carga.PotenciaAtiva;
            set
            {
                if (Carga.PotenciaAtiva == value)
                    return;

                Carga.PotenciaAtiva = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaReativa
        {
            get => Carga.PotenciaReativa;
            set
            {
                if (Carga.PotenciaReativa == value)
                    return;

                Carga.PotenciaReativa = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public int Alimentador
        {
            get => Carga.Alimentador;
            set
            {
                if (Carga.Alimentador == value)
                    return;

                Carga.Alimentador = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteLinha
        {
            get => Carga.CorrenteLinha;
            set
            {
                if (Carga.CorrenteLinha == value)
                    return;

                Carga.CorrenteLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseA
        {
            get => Carga.CorrenteFaseA;
            set
            {
                if (Carga.CorrenteFaseA == value)
                    return;

                Carga.CorrenteFaseA = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseB
        {
            get => Carga.CorrenteFaseB;
            set
            {
                if (Carga.CorrenteFaseB == value)
                    return;

                Carga.CorrenteFaseB = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseC
        {
            get => Carga.CorrenteFaseC;
            set
            {
                if (Carga.CorrenteFaseC == value)
                    return;

                Carga.CorrenteFaseC = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoLinha
        {
            get => Carga.TensaoLinha;
            set
            {
                if (Carga.TensaoLinha == value)
                    return;

                Carga.TensaoLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseA
        {
            get => Carga.TensaoFaseA;
            set
            {
                if (Carga.TensaoFaseA == value)
                    return;

                Carga.TensaoFaseA = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseB
        {
            get => Carga.TensaoFaseB;
            set
            {
                if (Carga.TensaoFaseB == value)
                    return;

                Carga.TensaoFaseB = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseC
        {
            get => Carga.TensaoFaseC;
            set
            {
                if (Carga.TensaoFaseC == value)
                    return;

                Carga.TensaoFaseC = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        protected override void AtualizarNode()
        {
            base.AtualizarNode();
            AtualizarTerminais();
        }

        public override void Mover(Vector delta)
        {
            base.Mover(delta);
            AtualizarTerminais();
        }

        private void AtualizarTerminais()
        {
            Carga.AtualizarTerminais(Largura);
        }
    }
}
