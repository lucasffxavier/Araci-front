using System.Windows;

using Araci.Models;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public abstract class ElementoViewModel
        : ViewModelBase
    {
        // =========================
        // MODELO
        // =========================

        protected readonly Elemento
            _modelo;

        // =========================
        // SELEÇÃO
        // =========================

        private bool _isSelecionado;

        public bool IsSelecionado
        {
            get => _isSelecionado;

            set
            {
                Set(
                    ref _isSelecionado,
                    value);
            }
        }

        // =========================
        // CONSTRUTOR
        // =========================

        protected ElementoViewModel(
            Elemento modelo)
        {
            _modelo = modelo;
        }

        // =========================
        // MODELO
        // =========================

        public Elemento Modelo =>
            _modelo;

        // =========================
        // POSIÇÃO
        // =========================

        public virtual double X
        {
            get => _modelo.PosicaoX;

            set
            {
                if (_modelo.PosicaoX != value)
                {
                    _modelo.PosicaoX = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Bounds));
                    OnPropertyChanged(nameof(Centro));
                }
            }
        }

        public virtual double Y
        {
            get => _modelo.PosicaoY;

            set
            {
                if (_modelo.PosicaoY != value)
                {
                    _modelo.PosicaoY = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Bounds));
                    OnPropertyChanged(nameof(Centro));
                }
            }
        }

        // =========================
        // DIMENSÕES
        // =========================

        public virtual double Largura =>
            70;

        public virtual double Altura =>
            70;

        // =========================
        // CENTRO
        // =========================

        public virtual Point Centro =>
            new(
                X + (Largura / 2.0),
                Y + (Altura / 2.0));

        // =========================
        // BOUNDS
        // =========================

        public virtual Rect Bounds =>
            new(
                X,
                Y,
                Largura,
                Altura);

        // =========================
        // MOVER
        // =========================

        public virtual void Mover(
            Vector delta)
        {
            X += delta.X;
            Y += delta.Y;
        }

        // =========================
        // ESTADO
        // =========================

        public virtual ElementoEstado
            CapturarEstado()
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