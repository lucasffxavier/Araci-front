using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Core.Rendering;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Properties;
using Araci.Services;
using Araci.ViewModels.Base;
using Araci.ViewModels.VisualStates;

namespace Araci.ViewModels
{
    public abstract class ElementoViewModel : ViewModelBase
    {
        private readonly ElementoNode _node;
        private ICommand? _abrirPropriedadesTipoCommand;
        private ObservableCollection<ParameterViewModel>? _parametros;

        protected ElementoViewModel(Elemento modelo, ElementoNode node, TypeLibraryService types)
        {
            Modelo = modelo ?? throw new ArgumentNullException(nameof(modelo));
            _node = node ?? throw new ArgumentNullException(nameof(node));
            Types = types ?? throw new ArgumentNullException(nameof(types));

            VisualState = new ElementoVisualState();

            _node.AtualizarGeometria();
        }

        public Elemento Modelo { get; }

        public ElementoNode Node => _node;

        protected TypeLibraryService Types { get; }

        public TipoElemento Tipo
        {
            get => Modelo.Tipo!;
            set
            {
                if (ReferenceEquals(Modelo.Tipo, value))
                    return;

                Modelo.Tipo = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(TipoViewModel));
            }
        }

        public abstract IEnumerable TiposDisponiveis { get; }

        protected void SelecionarPrimeiroTipoDisponivel()
        {
            TipoElemento? primeiroTipo = null;
            bool tipoAtualExiste = false;

            foreach (object? item in TiposDisponiveis)
            {
                if (item is not TipoElemento tipo)
                    continue;

                primeiroTipo ??= tipo;

                if (ReferenceEquals(tipo, Modelo.Tipo))
                {
                    tipoAtualExiste = true;
                    break;
                }
            }

            if (tipoAtualExiste || primeiroTipo == null)
                return;

            Tipo = primeiroTipo;
        }

        public virtual TipoElementoViewModel? TipoViewModel =>
            TipoElementoViewModelFactory.Criar(Tipo);

        public ICommand AbrirPropriedadesTipoCommand =>
            _abrirPropriedadesTipoCommand ??=
                new RelayCommand(AbrirPropriedadesTipo);

        private void AbrirPropriedadesTipo()
        {
            var janela = new TypePropertiesWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = TipoViewModel
            };

            janela.ShowDialog();
        }

        public ElementoVisualState VisualState { get; }

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

        public bool IsHover
        {
            get => VisualState.IsHover;
            set
            {
                if (VisualState.IsHover == value)
                    return;

                VisualState.AtualizarHover(value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(Stroke));
                OnPropertyChanged(nameof(StrokeThickness));
                OnPropertyChanged(nameof(RenderData));
            }
        }

        public Brush Stroke => VisualState.Stroke;

        public double StrokeThickness => VisualState.StrokeThickness;

        public virtual ElementoRenderData RenderData => new(
            Largura,
            Altura,
            PontoLocalInicial,
            PontoLocalFinal,
            Stroke,
            StrokeThickness);

        protected virtual Point PontoLocalInicial => new(0, 0);

        protected virtual Point PontoLocalFinal => new(Largura, Altura);

        public virtual double X
        {
            get => Node.X;
            set
            {
                if (Math.Abs(Node.X - value) < 0.0001)
                    return;

                Node.X = value;

                AtualizarNode();
            }
        }

        public virtual double Y
        {
            get => Node.Y;
            set
            {
                if (Math.Abs(Node.Y - value) < 0.0001)
                    return;

                Node.Y = value;

                AtualizarNode();
            }
        }

        public virtual double WorldX => Node.X;

        public virtual double WorldY => Node.Y;

        public virtual double Largura => Node.Largura;

        public virtual double Altura => Node.Altura;

        public virtual Rect Bounds => Node.Bounds;

        public virtual Point Centro => Node.Centro;

        protected virtual void AtualizarNode()
        {
            Node.AtualizarGeometria();

            NotificarGeometria();
        }

        protected virtual void NotificarGeometria()
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

        public virtual void Mover(Vector delta)
        {
            Node.Mover(delta);

            NotificarGeometria();
        }

        public virtual ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(X, Y);
        }

        public virtual void AplicarEstado(ElementoEstado estado)
        {
            Node.X = estado.X;
            Node.Y = estado.Y;

            AtualizarNode();
        }

        public ObservableCollection<ParameterViewModel> Parametros =>
            _parametros ??=
                new ObservableCollection<ParameterViewModel>(
                    Modelo.Parametros.Values.Select(
                        p => new ParameterViewModel(p)));

        public virtual void NotificarParametros()
        {
            foreach (var parametro in Parametros)
                parametro.Atualizar();

            OnPropertyChanged(nameof(Parametros));
        }
    }
}