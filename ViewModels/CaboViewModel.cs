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
        // =========================
        // CONSTRUTOR
        // =========================

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

        // =========================
        // MODELO
        // =========================

        public Cabo Cabo =>
            (Cabo)Modelo;

        private CaboNode CaboNode =>
            (CaboNode)Node;

        // =========================
        // TIPOS
        // =========================

        public override IEnumerable TiposDisponiveis =>
            Types.TiposCabos;

        // =========================
        // EXTREMIDADE FINAL
        // =========================

        public double X2
        {
            get => Cabo.PosicaoX2;

            set
            {
                if (Cabo.PosicaoX2 == value)
                    return;

                Cabo.PosicaoX2 = value;

                CaboNode.AtualizarGeometria();

                NotificarGeometria();

                OnPropertyChanged(nameof(X2));
            }
        }

        public double Y2
        {
            get => Cabo.PosicaoY2;

            set
            {
                if (Cabo.PosicaoY2 == value)
                    return;

                Cabo.PosicaoY2 = value;

                CaboNode.AtualizarGeometria();

                NotificarGeometria();

                OnPropertyChanged(nameof(Y2));
            }
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


        // =========================
        // PONTOS LOCAIS
        // =========================

        protected override Point
            PontoLocalInicial =>
            new(
                Cabo.PosicaoX - Bounds.X,
                Cabo.PosicaoY - Bounds.Y);

        protected override Point
            PontoLocalFinal =>
            new(
                Cabo.PosicaoX2 - Bounds.X,
                Cabo.PosicaoY2 - Bounds.Y);

        // =========================
        // MOVIMENTAÇÃO
        // =========================

        public override void Mover(
            Vector delta)
        {
            base.Mover(delta);

            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }

        // =========================
        // ESTADO
        // =========================

        public override ElementoEstado
            CapturarEstado()
        {
            return new ElementoEstado(
                X,
                Y,
                X2,
                Y2);
        }

        public override void AplicarEstado(
            ElementoEstado estado)
        {
            Cabo.PosicaoX = estado.X;
            Cabo.PosicaoY = estado.Y;

            Cabo.PosicaoX2 = estado.X2;
            Cabo.PosicaoY2 = estado.Y2;

            CaboNode.AtualizarGeometria();

            NotificarGeometria();

            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }
    }
}