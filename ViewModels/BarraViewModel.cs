using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class BarraViewModel : ElementoViewModel
    {
        public BarraViewModel(Barra modelo, TypeLibraryService types)
            : base(modelo, new BarraNode(modelo), types)
        {
            SelecionarPrimeiroTipoDisponivel();
            AtualizarTerminais();
        }

        public Barra Barra => (Barra)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposBarras;

        public string Nome
        {
            get => Barra.Nome;
            set
            {
                if (Barra.Nome == value)
                    return;

                Barra.Nome = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Tensao
        {
            get => Barra.Tensao;
            set
            {
                if (Barra.Tensao == value)
                    return;

                Barra.Tensao = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public override double Altura
        {
            get => Barra.Altura;
            set
            {
                if (Barra.Altura == value)
                    return;

                Barra.Altura = value;
                OnPropertyChanged();
                NotificarParametros();
                AtualizarNode();
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
            Barra.AtualizarTerminais();
        }
    }
}
