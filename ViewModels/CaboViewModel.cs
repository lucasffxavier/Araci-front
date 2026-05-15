using Araci.Models;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace Araci.ViewModels
{
    public class CaboViewModel
        : ElementoViewModel
    {
        private readonly Cabo _cabo;

        public override IEnumerable TiposDisponiveis =>
    AppServices.Types.TiposCabos;
        public CaboViewModel(
            Cabo cabo)
            : base(cabo)
        {
            _cabo = cabo
                ?? throw new ArgumentNullException(nameof(cabo));

            // =========================
            // GARANTE SEGUNDO PONTO
            // =========================

            if (_cabo.PosicaoX2 == 0 &&
                _cabo.PosicaoY2 == 0)
            {
                _cabo.PosicaoX2 =
                    _cabo.PosicaoX + 120;

                _cabo.PosicaoY2 =
                    _cabo.PosicaoY;
            }

            // =========================
            // VISUAL BASE
            // =========================

            VisualState.DefinirVisualBase(
                Brushes.Black,
                4);

            AtualizarGeometria();
        }

        // =========================
        // EXTREMIDADE FINAL
        // =========================

        public double X2
        {
            get => _cabo.PosicaoX2;

            set
            {
                if (_cabo.PosicaoX2 == value)
                    return;

                _cabo.PosicaoX2 = value;

                AtualizarGeometria();
            }
        }

        public double Y2
        {
            get => _cabo.PosicaoY2;

            set
            {
                if (_cabo.PosicaoY2 == value)
                    return;

                _cabo.PosicaoY2 = value;

                AtualizarGeometria();
            }
        }

        // =========================
        // DADOS
        // =========================

        public string Nome
        {
            get => _cabo.Nome;

            set
            {
                if (_cabo.Nome == value)
                    return;

                _cabo.Nome = value;

                OnPropertyChanged();
            }
        }

        public string BarraOrigem
        {
            get => _cabo.BarraOrigem;

            set
            {
                if (_cabo.BarraOrigem == value)
                    return;

                _cabo.BarraOrigem = value;

                OnPropertyChanged();
            }
        }

        public string BarraDestino
        {
            get => _cabo.BarraDestino;

            set
            {
                if (_cabo.BarraDestino == value)
                    return;

                _cabo.BarraDestino = value;

                OnPropertyChanged();
            }
        }

        public double Comprimento
        {
            get => _cabo.Comprimento;

            set
            {
                if (_cabo.Comprimento == value)
                    return;

                _cabo.Comprimento = value;

                OnPropertyChanged();
            }
        }


        // =========================
        // GEOMETRIA
        // =========================

        protected override void AtualizarGeometria()
        {
            Geometry.AtualizarLinha(
                X,
                Y,
                X2,
                Y2);

            NotificarGeometria();

            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }

        // =========================
        // MOVIMENTO COM LIMITE
        // =========================

        public override void Mover(
            Vector delta)
        {
            double novoX1 =
                X + delta.X;

            double novoY1 =
                Y + delta.Y;

            double novoX2 =
                X2 + delta.X;

            double novoY2 =
                Y2 + delta.Y;

            // =========================
            // LIMITES VIEWPORT
            // =========================

            if (AppServices.Viewport != null)
            {
                double larguraViewport =
                    AppServices.Viewport.Largura;

                double alturaViewport =
                    AppServices.Viewport.Altura;

                double minX =
                    Math.Min(novoX1, novoX2);

                double maxX =
                    Math.Max(novoX1, novoX2);

                double minY =
                    Math.Min(novoY1, novoY2);

                double maxY =
                    Math.Max(novoY1, novoY2);

                // =========================
                // AJUSTE HORIZONTAL
                // =========================

                if (minX < 0)
                {
                    double ajuste = -minX;

                    novoX1 += ajuste;
                    novoX2 += ajuste;
                }

                if (maxX > larguraViewport)
                {
                    double ajuste =
                        maxX - larguraViewport;

                    novoX1 -= ajuste;
                    novoX2 -= ajuste;
                }

                // =========================
                // AJUSTE VERTICAL
                // =========================

                if (minY < 0)
                {
                    double ajuste = -minY;

                    novoY1 += ajuste;
                    novoY2 += ajuste;
                }

                if (maxY > alturaViewport)
                {
                    double ajuste =
                        maxY - alturaViewport;

                    novoY1 -= ajuste;
                    novoY2 -= ajuste;
                }
            }

            // =========================
            // APLICA POSIÇÕES
            // =========================

            Transform.X =
                novoX1;

            Transform.Y =
                novoY1;

            _modelo.PosicaoX =
                novoX1;

            _modelo.PosicaoY =
                novoY1;

            _cabo.PosicaoX2 =
                novoX2;

            _cabo.PosicaoY2 =
                novoY2;

            AtualizarGeometria();
        }

        // =========================
        // SNAPSHOT
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

        // =========================
        // RESTAURAÇÃO
        // =========================

        public override void AplicarEstado(
            ElementoEstado estado)
        {
            Transform.X =
                estado.X;

            Transform.Y =
                estado.Y;

            _modelo.PosicaoX =
                estado.X;

            _modelo.PosicaoY =
                estado.Y;

            if (estado.PossuiSegundoPonto)
            {
                _cabo.PosicaoX2 =
                    estado.X2;

                _cabo.PosicaoY2 =
                    estado.Y2;
            }

            AtualizarGeometria();
        }
    }
}