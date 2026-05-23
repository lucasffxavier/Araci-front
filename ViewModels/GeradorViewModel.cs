using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class GeradorViewModel : ElementoViewModel
    {
        public GeradorViewModel(Gerador modelo, TypeLibraryService types)
            : base(modelo, new EquipamentoNode(modelo), types)
        {
            SelecionarPrimeiroTipoDisponivel();
            AtualizarTerminais();
        }

        public Gerador Gerador => (Gerador)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposGeradores;

        public string Nome
        {
            get => Gerador.Nome;
            set
            {
                if (Gerador.Nome == value)
                    return;

                Gerador.Nome = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaAparenteKVA
        {
            get => Gerador.PotenciaAparenteKVA;
            set
            {
                if (Gerador.PotenciaAparenteKVA == value)
                    return;

                Gerador.PotenciaAparenteKVA = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaAtivaKW
        {
            get => Gerador.PotenciaAtivaKW;
            set
            {
                if (Gerador.PotenciaAtivaKW == value)
                    return;

                Gerador.PotenciaAtivaKW = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaReativaKvar
        {
            get => Gerador.PotenciaReativaKvar;
            set
            {
                if (Gerador.PotenciaReativaKvar == value)
                    return;

                Gerador.PotenciaReativaKvar = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoLinha
        {
            get => Gerador.TensaoLinha;
            set
            {
                if (Gerador.TensaoLinha == value)
                    return;

                Gerador.TensaoLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseA
        {
            get => Gerador.TensaoFaseA;
            set
            {
                if (Gerador.TensaoFaseA == value)
                    return;

                Gerador.TensaoFaseA = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseB
        {
            get => Gerador.TensaoFaseB;
            set
            {
                if (Gerador.TensaoFaseB == value)
                    return;

                Gerador.TensaoFaseB = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseC
        {
            get => Gerador.TensaoFaseC;
            set
            {
                if (Gerador.TensaoFaseC == value)
                    return;

                Gerador.TensaoFaseC = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteLinha
        {
            get => Gerador.CorrenteLinha;
            set
            {
                if (Gerador.CorrenteLinha == value)
                    return;

                Gerador.CorrenteLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseA
        {
            get => Gerador.CorrenteFaseA;
            set
            {
                if (Gerador.CorrenteFaseA == value)
                    return;

                Gerador.CorrenteFaseA = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseB
        {
            get => Gerador.CorrenteFaseB;
            set
            {
                if (Gerador.CorrenteFaseB == value)
                    return;

                Gerador.CorrenteFaseB = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseC
        {
            get => Gerador.CorrenteFaseC;
            set
            {
                if (Gerador.CorrenteFaseC == value)
                    return;

                Gerador.CorrenteFaseC = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Alimentador
        {
            get => Gerador.Alimentador;
            set
            {
                if (Gerador.Alimentador == value)
                    return;

                Gerador.Alimentador = value;
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
            Gerador.AtualizarTerminais(Largura, Altura);
        }
    }
}
