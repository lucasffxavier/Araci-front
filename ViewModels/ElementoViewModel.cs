using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Araci.Core.Rendering;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Properties;
using Araci.Services;
using Araci.ViewModels.Base;
using Araci.ViewModels.VisualStates;

namespace Araci.ViewModels
{
    public abstract class ElementoViewModel
        : ViewModelBase
    {
        protected readonly Elemento _modelo;

        // ====================================================
        // CONSTRUTOR
        // ====================================================

        protected ElementoViewModel(
            Elemento modelo,
            TypeLibraryService types)
        {
            _modelo = modelo
                ?? throw new ArgumentNullException(nameof(modelo));

            Types = types
                ?? throw new ArgumentNullException(nameof(types));

            Transform =
                new ElementoTransform(
                    modelo.PosicaoX,
                    modelo.PosicaoY);

            VisualState =
                new ElementoVisualState();

            Geometry =
                new ElementoGeometryState();
        }

        // ====================================================
        // MODELO
        // ====================================================

        public Elemento Modelo =>
            _modelo;

        protected TypeLibraryService Types
        { get; }

        // ====================================================
        // TIPO
        // ====================================================

        public TipoElemento Tipo
        {
            get => _modelo.Tipo!;

            set
            {
                if (_modelo.Tipo == value)
                    return;

                _modelo.Tipo = value;

                OnPropertyChanged();

                OnPropertyChanged(nameof(TipoViewModel));
            }
        }

        public abstract IEnumerable
            TiposDisponiveis
        { get; }

        protected void SelecionarPrimeiroTipoDisponivel()
        {
            TipoElemento? primeiroTipo =
                null;

            bool tipoAtualExisteNaLista =
                false;

            foreach (object? item in TiposDisponiveis)
            {
                if (item is not TipoElemento tipo)
                    continue;

                primeiroTipo ??=
                    tipo;

                if (ReferenceEquals(tipo, _modelo.Tipo))
                {
                    tipoAtualExisteNaLista =
                        true;

                    break;
                }
            }

            if (tipoAtualExisteNaLista ||
                primeiroTipo == null)
            {
                return;
            }

            Tipo =
                primeiroTipo;
        }

        public virtual TipoElementoViewModel?
            TipoViewModel =>
                TipoElementoViewModelFactory
                    .Criar(Tipo);

        // ====================================================
        // COMANDO
        // ====================================================

        private ICommand? _abrirPropriedadesTipoCommand;

        public ICommand AbrirPropriedadesTipoCommand =>
            _abrirPropriedadesTipoCommand ??=
                new RelayCommand(AbrirPropriedadesTipo);

        private void AbrirPropriedadesTipo()
        {
            var janela =
                new TypePropertiesWindow
                {
                    Owner =
                        Application.Current.MainWindow,

                    DataContext =
                        TipoViewModel
                };

            janela.ShowDialog();
        }

        // ====================================================
        // TRANSFORM
        // ====================================================

        public ElementoTransform Transform
        { get; }

        // ====================================================
        // VISUAL
        // ====================================================

        public ElementoVisualState
            VisualState
        { get; }

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

                OnPropertyChanged(nameof(RenderData));
            }
        }

        public Brush Stroke =>
            VisualState.Stroke;

        public double StrokeThickness =>
            VisualState.StrokeThickness;

        // ====================================================
        // RENDER
        // ====================================================

        public virtual ElementoRenderData RenderData =>
            new ElementoRenderData(
                Largura,
                Altura,
                Geometry.PontoLocalInicial,
                Geometry.PontoLocalFinal,
                Stroke,
                StrokeThickness);

        // ====================================================
        // GEOMETRIA
        // ====================================================

        public ElementoGeometryState
            Geometry
        { get; }

        protected virtual double
            LarguraBase =>
                70;

        protected virtual double
            AlturaBase =>
                70;

        public virtual double
            Largura =>
                Geometry.Largura;

        public virtual double
            Altura =>
                Geometry.Altura;

        public virtual Rect
            Bounds =>
                Geometry.Bounds;

        public virtual Point
            Centro =>
                Geometry.Centro;

        protected virtual void
            AtualizarGeometria()
        {
            Geometry.AtualizarRetangulo(
                X,
                Y,
                LarguraBase,
                AlturaBase);

            NotificarGeometria();
        }

        protected void
            NotificarGeometria()
        {
            OnPropertyChanged(nameof(X));

            OnPropertyChanged(nameof(Y));

            OnPropertyChanged(nameof(WorldX));

            OnPropertyChanged(nameof(WorldY));

            OnPropertyChanged(nameof(Largura));

            OnPropertyChanged(nameof(Altura));

            OnPropertyChanged(nameof(Bounds));

            OnPropertyChanged(nameof(Centro));

            OnPropertyChanged(nameof(RenderData));
        }

        // ====================================================
        // POSIÇÃO EM MUNDO
        // ====================================================

        public double WorldX =>
            X;

        public double WorldY =>
            Y;

        public virtual double X
        {
            get => Transform.X;

            set
            {
                if (Transform.X == value)
                    return;

                Transform.X =
                    value;

                _modelo.PosicaoX =
                    value;

                AtualizarGeometria();
            }
        }

        public virtual double Y
        {
            get => Transform.Y;

            set
            {
                if (Transform.Y == value)
                    return;

                Transform.Y =
                    value;

                _modelo.PosicaoY =
                    value;

                AtualizarGeometria();
            }
        }

        // ====================================================
        // MOVIMENTO
        // ====================================================

        public virtual void Mover(
            Vector delta)
        {
            X += delta.X;

            Y += delta.Y;
        }

        // ====================================================
        // SNAPSHOT
        // ====================================================

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
            Transform.X =
                estado.X;

            Transform.Y =
                estado.Y;

            _modelo.PosicaoX =
                estado.X;

            _modelo.PosicaoY =
                estado.Y;

            AtualizarGeometria();
        }
    }
}
