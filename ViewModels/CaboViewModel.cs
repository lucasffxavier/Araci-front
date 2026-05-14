using System;
using System.Windows;
using System.Windows.Media;

using Araci.Models;

namespace Araci.ViewModels
{
    public class CaboViewModel
        : ElementoViewModel
    {
        private readonly Cabo _cabo;

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

        public string TipoCabo
        {
            get => _cabo.TipoCabo;

            set
            {
                if (_cabo.TipoCabo == value)
                    return;

                _cabo.TipoCabo = value;

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
        // MOVIMENTO
        // =========================

        public override void Mover(
            Vector delta)
        {
            // =========================
            // MOVE PONTO INICIAL
            // =========================

            Transform.Mover(delta);

            _modelo.PosicaoX =
                Transform.X;

            _modelo.PosicaoY =
                Transform.Y;

            // =========================
            // MOVE PONTO FINAL
            // =========================

            _cabo.PosicaoX2 +=
                delta.X;

            _cabo.PosicaoY2 +=
                delta.Y;

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
            // =========================
            // PONTO INICIAL
            // =========================

            Transform.X =
                estado.X;

            Transform.Y =
                estado.Y;

            _modelo.PosicaoX =
                estado.X;

            _modelo.PosicaoY =
                estado.Y;

            // =========================
            // PONTO FINAL
            // =========================

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