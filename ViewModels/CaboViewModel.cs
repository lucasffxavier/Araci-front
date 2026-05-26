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
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new CaboNode(modelo), types, names, typePropertiesDialogs)
        {
            SelecionarPrimeiroTipoDisponivel();
        }

        public Cabo Cabo => (Cabo)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposCabos;

        public string OrigemId
        {
            get => Cabo.OrigemId;
            set
            {
                if (Cabo.OrigemId == value)
                    return;

                Cabo.OrigemId = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string DestinoId
        {
            get => Cabo.DestinoId;
            set
            {
                if (Cabo.DestinoId == value)
                    return;

                Cabo.DestinoId = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Nome
        {
            get => Cabo.Nome;
            set
            {
                if (Cabo.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
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
                NotificarParametros();
            }
        }

        public double Ampacidade
        {
            get => Cabo.Ampacidade;
            set
            {
                if (Cabo.Ampacidade == value)
                    return;

                Cabo.Ampacidade = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoLinha
        {
            get => Cabo.TensaoLinha;
            set
            {
                if (Cabo.TensaoLinha == value)
                    return;

                Cabo.TensaoLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseA
        {
            get => Cabo.TensaoFaseA;
            set
            {
                if (Cabo.TensaoFaseA == value)
                    return;

                Cabo.TensaoFaseA = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseB
        {
            get => Cabo.TensaoFaseB;
            set
            {
                if (Cabo.TensaoFaseB == value)
                    return;

                Cabo.TensaoFaseB = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoFaseC
        {
            get => Cabo.TensaoFaseC;
            set
            {
                if (Cabo.TensaoFaseC == value)
                    return;

                Cabo.TensaoFaseC = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteLinha
        {
            get => Cabo.CorrenteLinha;
            set
            {
                if (Cabo.CorrenteLinha == value)
                    return;

                Cabo.CorrenteLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseA
        {
            get => Cabo.CorrenteFaseA;
            set
            {
                if (Cabo.CorrenteFaseA == value)
                    return;

                Cabo.CorrenteFaseA = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseB
        {
            get => Cabo.CorrenteFaseB;
            set
            {
                if (Cabo.CorrenteFaseB == value)
                    return;

                Cabo.CorrenteFaseB = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorrenteFaseC
        {
            get => Cabo.CorrenteFaseC;
            set
            {
                if (Cabo.CorrenteFaseC == value)
                    return;

                Cabo.CorrenteFaseC = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public override double WorldX => Bounds.X;

        public override double WorldY => Bounds.Y;

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

            if (Cabo.PreviewPonto.HasValue && Cabo.PreviewPonto.Value == p)
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
            if (!_possuiPreview && Cabo.PreviewPonto == null)
                return;

            _possuiPreview = false;
            Cabo.PreviewPonto = null;
            Atualizar();
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
            return new ElementoEstado(WorldX, WorldY, vertices: Cabo.Vertices);
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

        public override void AtualizarAposModeloAlterado()
        {
            Atualizar();
        }

        private CaboNode CaboNode => (CaboNode)Node;

        private void Atualizar()
        {
            CaboNode.AtualizarGeometria();

            OnPropertyChanged(nameof(WorldX));
            OnPropertyChanged(nameof(WorldY));
            OnPropertyChanged(nameof(Bounds));

            NotificarGeometria();
        }
    }
}
