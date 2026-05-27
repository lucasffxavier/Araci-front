using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class SinViewModel : ElementoViewModel
    {
        private readonly TerminalLayoutService _terminalLayout;

        public SinViewModel(
            Sin modelo,
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

        public Sin Sin => (Sin)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposSin;

        public string Nome
        {
            get => Sin.Nome;
            set
            {
                if (Sin.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Barra
        {
            get => Sin.Barra;
            set
            {
                if (Sin.Barra == value)
                    return;

                Sin.Barra = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoLinha
        {
            get => Sin.TensaoLinha;
            set
            {
                if (Sin.TensaoLinha == value)
                    return;

                Sin.TensaoLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public int Alimentador
        {
            get => Sin.Alimentador;
            set
            {
                if (Sin.Alimentador == value)
                    return;

                Sin.Alimentador = value;
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
            _terminalLayout.AtualizarTerminais(Sin);
        }
    }
}
