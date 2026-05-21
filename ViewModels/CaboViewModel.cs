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

        public CaboViewModel(
            Cabo modelo,
            TypeLibraryService types)
            : base(modelo, new CaboNode(modelo), types)
        {
            SelecionarPrimeiroTipoDisponivel();
        }

        public Cabo Cabo => (Cabo)Modelo;

        private CaboNode CaboNode => (CaboNode)Node;

        public override IEnumerable TiposDisponiveis =>
            Types.TiposCabos;

        public void Iniciar(Point p)
        {
            Cabo.Vertices.Clear();

            Cabo.Vertices.Add(p);

            Cabo.DefinirOrigem(p);

            Cabo.PreviewPonto = p;

            _possuiPreview = true;

            Atualizar();
        }

        public void AtualizarPreview(Point p)
        {
            if (!_possuiPreview)
                return;

            if (Cabo.PreviewPonto.HasValue &&
                Cabo.PreviewPonto.Value == p)
                return;

            Cabo.PreviewPonto = p;

            Atualizar();
        }

        public void FinalizarNoPonto(Point p)
        {
            if (Cabo.Vertices.Count == 0)
                return;

            Cabo.PreviewPonto = null;
            _possuiPreview = false;

            var ultimo = Cabo.Vertices[^1];

            if (ultimo != p)
                Cabo.Vertices.Add(p);

            Cabo.DefinirDestino(p);

            Atualizar();
        }

        public void RemoverPreview()
        {
            if (!_possuiPreview &&
                Cabo.PreviewPonto == null)
                return;

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
                    return;

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
                    return;

                Cabo.BarraOrigem = value;

                OnPropertyChanged();

                NotificarParametros();
            }
        }

        public string BarraDestino
        {
            get => Cabo.BarraDestino;
            set
            {
                if (Cabo.BarraDestino == value)
                    return;

                Cabo.BarraDestino = value;

                OnPropertyChanged();

                NotificarParametros();
            }
        }

        public double Comprimento
        {
            get => Cabo.Comprimento;
            set
            {
                if (Cabo.Comprimento == value)
                    return;

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
            {
                t.Posicao = new Point(
                    t.Posicao.X + delta.X,
                    t.Posicao.Y + delta.Y);
            }

            Atualizar();
        }

        public override ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(
                WorldX,
                WorldY,
                vertices: Cabo.Vertices);
        }

        public override void AplicarEstado(ElementoEstado estado)
        {
            Cabo.Vertices.Clear();

            foreach (var p in estado.Vertices)
                Cabo.Vertices.Add(p);

            if (Cabo.Vertices.Count > 0)
            {
                Cabo.DefinirOrigem(Cabo.Vertices[0]);
                Cabo.DefinirDestino(Cabo.Vertices[^1]);
            }

            Atualizar();
        }

        public override double WorldX => Bounds.X;

        public override double WorldY => Bounds.Y;
    }
}