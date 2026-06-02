using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;
using Araci.Services.Geometry;
using Araci.Services.UI;
using Araci.Services.Catalog;
using Araci.Services.Naming;

namespace Araci.ViewModels
{
    public class GeradorViewModel : ElementoViewModel
    {
        private readonly TerminalLayoutService _terminalLayout;

        public GeradorViewModel(
            Gerador modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs,
            TerminalLayoutService terminalLayout)
            : base(modelo, new EquipamentoNode(modelo), types, names, typePropertiesDialogs)
        {
            _terminalLayout = terminalLayout;
            SelecionarPrimeiroTipoDisponivel();
            AtualizarTerminais();
        }

        public Gerador Gerador => (Gerador)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposGeradores;

        public string Barra
        {
            get => Gerador.Barra;
            set
            {
                if (Gerador.Barra == value)
                    return;

                Gerador.Barra = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string BarraId
        {
            get => Gerador.BarraId;
            set
            {
                if (Gerador.BarraId == value)
                    return;

                Gerador.BarraId = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Nome
        {
            get => Gerador.Nome;
            set
            {
                if (Gerador.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaAparente
        {
            get => Gerador.PotenciaAparente;
            set
            {
                if (Gerador.PotenciaAparente == value)
                    return;

                Gerador.PotenciaAparente = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaAtiva
        {
            get => Gerador.PotenciaAtiva;
            set
            {
                if (Gerador.PotenciaAtiva == value)
                    return;

                Gerador.PotenciaAtiva = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double PotenciaReativa
        {
            get => Gerador.PotenciaReativa;
            set
            {
                if (Gerador.PotenciaReativa == value)
                    return;

                Gerador.PotenciaReativa = value;
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

        public int Alimentador
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
            _terminalLayout.AtualizarTerminais(Gerador);
        }
    }
}
