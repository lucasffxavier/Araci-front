using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class TransformadorViewModel : ElementoViewModel
    {
        private readonly TerminalLayoutService _terminalLayout;

        public TransformadorViewModel(
            Transformador modelo,
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

        public Transformador Transformador => (Transformador)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposTransformadores;

        public string Nome
        {
            get => Transformador.Nome;
            set
            {
                if (Transformador.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Barra
        {
            get => Transformador.Barra;
            set
            {
                if (Transformador.Barra == value)
                    return;

                Transformador.Barra = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoLinha
        {
            get => Transformador.TensaoLinha;
            set
            {
                if (Transformador.TensaoLinha == value)
                    return;

                Transformador.TensaoLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public int Alimentador
        {
            get => Transformador.Alimentador;
            set
            {
                if (Transformador.Alimentador == value)
                    return;

                Transformador.Alimentador = value;
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
            _terminalLayout.AtualizarTerminais(Transformador);
        }
    }
}
