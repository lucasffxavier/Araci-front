using Araci.Models;
using Araci.ViewModels.Base;
using Araci.ViewModels.VisualStates;

using System;
using System.Windows;
using System.Windows.Media;

namespace Araci.ViewModels
{
    public abstract class ElementoViewModel
        : ViewModelBase
    {
        protected readonly Elemento _modelo;

        public ElementoTransform Transform { get; }

        public ElementoVisualState VisualState { get; }

        public ElementoGeometryState Geometry { get; }

        protected ElementoViewModel(
            Elemento modelo)
        {
            _modelo = modelo
                ?? throw new ArgumentNullException(nameof(modelo));

            Transform =
                new ElementoTransform(
                    modelo.PosicaoX,
                    modelo.PosicaoY);

            VisualState =
                new ElementoVisualState();

            Geometry =
                new ElementoGeometryState();
        }

        public Elemento Modelo => _modelo;

        // =========================
        // VISUAL
        // =========================

        public bool IsSelecionado
        {
            get => VisualState.IsSelecionado;

            set
            {
                if (VisualState.IsSelecionado == value)
                    return;

                VisualState.AtualizarSelecao(value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(Stroke));
                OnPropertyChanged(nameof(StrokeThickness));
            }
        }

        public Brush Stroke =>
            VisualState.Stroke;

        public double StrokeThickness =>
            VisualState.StrokeThickness;

        // =========================
        // POSIÇÃO
        // =========================

        public virtual double X
        {
            get => Transform.X;

            set
            {
                double valorLimitado =
                    LimitarX(value);

                if (Transform.X == valorLimitado)
                    return;

                Transform.X = valorLimitado;

                _modelo.PosicaoX = valorLimitado;

                AtualizarGeometria();
            }
        }

        public virtual double Y
        {
            get => Transform.Y;

            set
            {
                double valorLimitado =
                    LimitarY(value);

                if (Transform.Y == valorLimitado)
                    return;

                Transform.Y = valorLimitado;

                _modelo.PosicaoY = valorLimitado;

                AtualizarGeometria();
            }
        }

        // =========================
        // GEOMETRIA
        // =========================

        protected virtual double LarguraBase => 70;

        protected virtual double AlturaBase => 70;

        public virtual double Largura =>
            Geometry.Largura;

        public virtual double Altura =>
            Geometry.Altura;

        public virtual Rect Bounds =>
            Geometry.Bounds;

        public virtual Point Centro =>
            Geometry.Centro;

        protected virtual void AtualizarGeometria()
        {
            Geometry.Atualizar(
                X,
                Y,
                LarguraBase,
                AlturaBase);

            NotificarGeometria();
        }

        protected void NotificarGeometria()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Largura));
            OnPropertyChanged(nameof(Altura));
            OnPropertyChanged(nameof(Bounds));
            OnPropertyChanged(nameof(Centro));
        }

        // =========================
        // MOVIMENTO
        // =========================

        public virtual void Mover(
            Vector delta)
        {
            X += delta.X;
            Y += delta.Y;
        }

        // =========================
        // LIMITES
        // =========================

        private double LimitarX(
            double valor)
        {
            if (AppServices.Viewport == null)
                return valor;

            double max =
                Math.Max(
                    0,
                    AppServices.Viewport.Largura - Largura);

            return Math.Max(
                0,
                Math.Min(valor, max));
        }

        private double LimitarY(
            double valor)
        {
            if (AppServices.Viewport == null)
                return valor;

            double max =
                Math.Max(
                    0,
                    AppServices.Viewport.Altura - Altura);

            return Math.Max(
                0,
                Math.Min(valor, max));
        }

        // =========================
        // SNAPSHOT
        // =========================

        public virtual ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(
                X,
                Y);
        }

        public virtual void AplicarEstado(
            ElementoEstado estado)
        {
            Transform.X = estado.X;
            Transform.Y = estado.Y;

            _modelo.PosicaoX = estado.X;
            _modelo.PosicaoY = estado.Y;

            AtualizarGeometria();
        }
    }
}