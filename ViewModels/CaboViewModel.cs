using System.Collections;
using System.Windows;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class CaboViewModel : ElementoViewModel
    {
        private bool _possuiPreview;

        public CaboViewModel(Cabo modelo, TypeLibraryService types)
            : base(modelo, new CaboNode(modelo), types)
        {
            SelecionarPrimeiroTipoDisponivel();
        }

        public Cabo Cabo => (Cabo)Modelo;
        private CaboNode CaboNode => (CaboNode)Node;

        public override IEnumerable TiposDisponiveis => Types.TiposCabos;

        public void Iniciar(Point p)
        {
            Cabo.Vertices.Clear();
            Cabo.Vertices.Add(p);
            Cabo.DefinirOrigem(p);
            Cabo.PreviewPonto = p;
            _possuiPreview = true;
            Atualizar();
        }

        public void ConfirmarSegmento(Point p)
        {
            if (Cabo.Vertices.Count == 0) return;

            Cabo.Vertices.Add(p);
            Cabo.DefinirDestino(p);
            Cabo.PreviewPonto = p;
            _possuiPreview = true;

            Atualizar();
        }

        public void AtualizarPreview(Point p)
        {
            if (!_possuiPreview) return;

            Cabo.PreviewPonto = p;
            Atualizar();
        }

        public void RemoverPreview()
        {
            _possuiPreview = false;
            Cabo.PreviewPonto = null;
            Atualizar();
        }

        public string Nome
        {
            get => Cabo.Nome;

            set
            {
                if (Cabo.Nome == value)
                {
                    return;
                }

                Cabo.Nome = value;

                OnPropertyChanged();
            }
        }

        public string BarraOrigem
        {
            get => Cabo.BarraOrigem;

            set
            {
                if (Cabo.BarraOrigem == value)
                {
                    return;
                }

                Cabo.BarraOrigem = value;

                OnPropertyChanged();
            }
        }

        public string BarraDestino
        {
            get => Cabo.BarraDestino;

            set
            {
                if (Cabo.BarraDestino == value)
                {
                    return;
                }

                Cabo.BarraDestino = value;

                OnPropertyChanged();
            }
        }

        public double Comprimento
        {
            get => Cabo.Comprimento;

            set
            {
                if (Cabo.Comprimento == value)
                {
                    return;
                }

                Cabo.Comprimento = value;

                OnPropertyChanged();
            }
        }

        private void Atualizar()
        {
            CaboNode.AtualizarGeometria();
            OnPropertyChanged(nameof(WorldX));
            OnPropertyChanged(nameof(WorldY));
            OnPropertyChanged(nameof(Bounds));
            NotificarGeometria();
        }

        public override void Mover(Vector delta)
        {
            CaboNode.Mover(delta);

            foreach (var t in Cabo.Terminais)
                t.Posicao = new Point(t.Posicao.X + delta.X, t.Posicao.Y + delta.Y);

            Atualizar();
        }

        public override double WorldX => Bounds.X;
        public override double WorldY => Bounds.Y;
    }
}