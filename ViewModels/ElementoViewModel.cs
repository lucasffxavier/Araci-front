using System;
using System.Windows;

using Araci.Models;
using Araci.ViewModels.Base;
using Araci.ViewModels.VisualStates;

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

            Transform = new ElementoTransform(
                modelo.PosicaoX,
                modelo.PosicaoY);

            VisualState = new ElementoVisualState();

            Geometry = new ElementoGeometryState();
        }

        public Elemento Modelo => _modelo;

        public bool IsSelecionado
        {
            get => VisualState.IsSelecionado;

            set
            {
                if (VisualState.IsSelecionado == value)
                    return;

                VisualState.IsSelecionado = value;
                _modelo.Selecionado = value;

                OnPropertyChanged();
            }
        }

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

        public virtual double Largura => 70;

        public virtual double Altura => 70;

        public virtual Rect Bounds =>
            new Rect(
                X,
                Y,
                Largura,
                Altura);

        public virtual Point Centro =>
            new Point(
                X + (Largura / 2.0),
                Y + (Altura / 2.0));

        protected virtual void AtualizarGeometria()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Largura));
            OnPropertyChanged(nameof(Altura));
            OnPropertyChanged(nameof(Bounds));
            OnPropertyChanged(nameof(Centro));
        }

        public virtual void Mover(
            Vector delta)
        {
            X += delta.X;
            Y += delta.Y;
        }

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

        public virtual ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(
                X,
                Y);
        }

        public virtual void AplicarEstado(
            ElementoEstado estado)
        {
            X = estado.X;
            Y = estado.Y;
        }
    }
}