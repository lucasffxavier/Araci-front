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
using Araci.Services;
using Araci.ViewModels.Base;
using Araci.ViewModels.VisualStates;
using Araci.Services.UI;
using Araci.Services.Catalog;
using Araci.Services.Naming;

namespace Araci.ViewModels
{
    public abstract class ElementoViewModel : ViewModelBase
    {
        private readonly ElementoNode _node;
        private readonly NameService _names;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private ICommand? _abrirPropriedadesTipoCommand;
        private ObservableCollection<ParameterViewModel>? _parametros;
        private bool _isPreview;

        protected ElementoViewModel(Elemento modelo, ElementoNode node, TypeLibraryService types, NameService names, TypePropertiesDialogService typePropertiesDialogs)
        {
            Modelo = modelo ?? throw new ArgumentNullException(nameof(modelo));
            _node = node ?? throw new ArgumentNullException(nameof(node));
            Types = types ?? throw new ArgumentNullException(nameof(types));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));
            VisualState = new ElementoVisualState();
            _node.AtualizarGeometria();
        }

        public Elemento Modelo { get; }
        public ElementoNode Node => _node;
        protected TypeLibraryService Types { get; }

        protected void RenomearModelo(string novoNome)
        {
            _names.Renomear(Modelo, novoNome);
        }

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

        public virtual TipoElementoViewModel? TipoViewModel => TipoElementoViewModelFactory.Criar(Tipo);

        public ICommand AbrirPropriedadesTipoCommand => _abrirPropriedadesTipoCommand ??= new RelayCommand(AbrirPropriedadesTipo);

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

        public bool IsPreview
        {
            get => _isPreview;
            set
            {
                if (_isPreview == value)
                    return;

                _isPreview = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RenderData));
            }
        }

        public Brush Stroke => VisualState.Stroke;
        public double StrokeThickness => VisualState.StrokeThickness;

        public virtual ElementoRenderData RenderData => new(Largura, Altura, PontoLocalInicial, PontoLocalFinal, Stroke, StrokeThickness);

        public double Rotacao
        {
            get => Modelo.Rotacao;
            set
            {
                if (Math.Abs(Modelo.Rotacao - value) < 0.0001)
                    return;

                Modelo.Rotacao = value;
                AtualizarAposModeloAlterado();
            }
        }

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

        public virtual double Altura
        {
            get => Node.Altura;
            set { }
        }

        public virtual Rect Bounds => Node.Bounds;
        public virtual Rect BoundsAlinhamento => ObterBoundsRotacionado(Node.BoundsAlinhamento);
        public virtual Point Centro => Node.Centro;

        public ObservableCollection<ParameterViewModel> Parametros =>
            _parametros ??= new ObservableCollection<ParameterViewModel>(Modelo.Parametros.Values.Select(p => new ParameterViewModel(p)));

        protected virtual Point PontoLocalInicial => new(0, 0);
        protected virtual Point PontoLocalFinal => new(Largura, Altura);

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

        public virtual void Mover(Vector delta)
        {
            Node.Mover(delta);
            NotificarGeometria();
        }

        public virtual ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(X, Y, rotacao: Modelo.Rotacao);
        }

        public virtual void AplicarEstado(ElementoEstado estado)
        {
            Node.X = estado.X;
            Node.Y = estado.Y;
            AtualizarNode();
        }

        public virtual void AtualizarAposModeloAlterado()
        {
            AtualizarNode();
        }

        public virtual void NotificarParametros()
        {
            foreach (var parametro in Parametros)
                parametro.Atualizar();

            OnPropertyChanged(nameof(Parametros));
        }

        public void NotificarPropriedades(params string[] nomes)
        {
            foreach (string nome in nomes)
                OnPropertyChanged(nome);

            NotificarParametros();
        }

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
            OnPropertyChanged(nameof(BoundsAlinhamento));
            OnPropertyChanged(nameof(Centro));
            OnPropertyChanged(nameof(Rotacao));
            OnPropertyChanged(nameof(RenderData));
        }

        private Rect ObterBoundsRotacionado(Rect bounds)
        {
            if (Math.Abs(Rotacao) <= 0.000001)
                return bounds;

            Point center = Centro;
            Point p1 = RotateAround(new Point(bounds.Left, bounds.Top), center, Rotacao);
            Point p2 = RotateAround(new Point(bounds.Right, bounds.Top), center, Rotacao);
            Point p3 = RotateAround(new Point(bounds.Right, bounds.Bottom), center, Rotacao);
            Point p4 = RotateAround(new Point(bounds.Left, bounds.Bottom), center, Rotacao);
            double minX = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
            double minY = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
            double maxX = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
            double maxY = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));
            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        private static Point RotateAround(Point point, Point center, double angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double x = point.X - center.X;
            double y = point.Y - center.Y;
            return new Point(center.X + x * cos - y * sin, center.Y + x * sin + y * cos);
        }

        private void AbrirPropriedadesTipo()
        {
            _typePropertiesDialogs.Show(TipoViewModel);
        }
    }
}
