using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class CaboViewModel
        : ElementoViewModel
    {
        private bool _possuiPreview;

        public CaboViewModel(
            Cabo modelo,
            TypeLibraryService types)
            : base(
                modelo,
                new CaboNode(modelo),
                types)
        {
            SelecionarPrimeiroTipoDisponivel();
        }

        public Cabo Cabo =>
            (Cabo)Modelo;

        private CaboNode CaboNode =>
            (CaboNode)Node;

        public override IEnumerable TiposDisponiveis =>
            Types.TiposCabos;

        public void Iniciar(Point ponto)
        {
            Cabo.Vertices.Clear();

            Cabo.Vertices.Add(ponto);

            Cabo.PreviewPonto = ponto;

            _possuiPreview = true;

            AtualizarGeometria();
        }

        public void ConfirmarSegmento(Point ponto)
        {
            if (Cabo.Vertices.Count == 0)
            {
                return;
            }

            Cabo.Vertices.Add(ponto);

            Cabo.PreviewPonto = ponto;

            _possuiPreview = true;

            AtualizarGeometria();
        }

        public void AtualizarPreview(Point ponto)
        {
            if (!_possuiPreview)
            {
                return;
            }

            Cabo.PreviewPonto = ponto;

            AtualizarGeometria();
        }

        public void RemoverPreview()
        {
            _possuiPreview = false;

            Cabo.PreviewPonto = null;

            AtualizarGeometria();
        }

        private void AtualizarGeometria()
        {
            CaboNode.AtualizarGeometria();

            // 🔥 GARANTIR atualização de posição
            OnPropertyChanged(nameof(WorldX));
            OnPropertyChanged(nameof(WorldY));
            OnPropertyChanged(nameof(Bounds));

            NotificarGeometria();
        }

        public override void Mover(
            Vector delta)
        {
            CaboNode.Mover(delta);

            AtualizarGeometria();
        }

        public override double WorldX =>
            Bounds.X;

        public override double WorldY =>
            Bounds.Y;

        public override ElementoEstado
            CapturarEstado()
        {
            if (Cabo.Vertices.Count == 0)
            {
                return base.CapturarEstado();
            }

            Point p =
                Cabo.Vertices[0];

            return new ElementoEstado(
                p.X,
                p.Y);
        }

        public string Nome
        {
            get => Cabo.Nome;

            set
            {
                if (Cabo.Nome
                    == value)
                {
                    return;
                }

                Cabo.Nome =
                    value;

                OnPropertyChanged();
            }
        }


        public string BarraOrigem
        {
            get => Cabo.BarraOrigem;

            set
            {
                if (Cabo.BarraOrigem
                    == value)
                {
                    return;
                }

                Cabo.BarraOrigem =
                    value;

                OnPropertyChanged();
            }
        }

        public string BarraDestino
        {
            get => Cabo.Nome;

            set
            {
                if (Cabo.BarraDestino
                    == value)
                {
                    return;
                }

                Cabo.BarraDestino =
                    value;

                OnPropertyChanged();
            }
        }

        public double Comprimento
        {
            get => Cabo.Comprimento;

            set
            {
                if (Cabo.Comprimento
                    == value)
                {
                    return;
                }

                Cabo.Comprimento =
                    value;

                OnPropertyChanged();
            }
        }

    }
}