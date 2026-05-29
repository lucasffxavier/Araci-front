using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Araci.Controls.Base;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class CaboControl : ElementoControlBase
    {
        private const double VertexHandleSize = 12;
        private readonly Canvas _root;
        private readonly Polyline _polyline;
        private readonly Polyline _hitArea;
        private readonly List<Ellipse> _vertexHandles = new();
        private CaboViewModel? _vmAtual;

        public CaboControl()
        {
            ClipToBounds = false;

            _root = new Canvas
            {
                ClipToBounds = false,
                Background = Brushes.Transparent
            };

            _hitArea = new Polyline
            {
                Stroke = Brushes.Transparent,
                StrokeThickness = 12,
                IsHitTestVisible = true
            };

            _polyline = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 4,
                SnapsToDevicePixels = true,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };

            _root.Children.Add(_hitArea);
            _root.Children.Add(_polyline);
            Content = _root;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected override bool UsaBindings => false;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Conectar();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Desconectar();
        }

        private void Conectar()
        {
            Desconectar();

            if (DataContext is not CaboViewModel vm)
                return;

            _vmAtual = vm;
            vm.PropertyChanged += OnViewModelChanged;
            vm.Cabo.Vertices.CollectionChanged += OnVerticesChanged;
            AtualizarPolyline();
            AplicarEstadoVisual(vm);
        }

        private void Desconectar()
        {
            if (_vmAtual == null)
                return;

            _vmAtual.PropertyChanged -= OnViewModelChanged;
            _vmAtual.Cabo.Vertices.CollectionChanged -= OnVerticesChanged;
            _vmAtual = null;
        }

        private void OnVerticesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            AtualizarPolyline();
        }

        private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ElementoViewModel.Bounds) &&
                e.PropertyName != nameof(ElementoViewModel.WorldX) &&
                e.PropertyName != nameof(ElementoViewModel.WorldY) &&
                e.PropertyName != nameof(ElementoViewModel.RenderData) &&
                e.PropertyName != nameof(ElementoViewModel.IsPreview) &&
                e.PropertyName != nameof(ElementoViewModel.IsHover) &&
                e.PropertyName != nameof(ElementoViewModel.IsSelecionado))
            {
                return;
            }

            AtualizarPolyline();
            AplicarEstadoVisual((ElementoViewModel)sender!);
        }

        private void AtualizarPolyline()
        {
            if (_vmAtual == null)
                return;

            var cabo = _vmAtual.Cabo;
            _polyline.Points.Clear();
            _hitArea.Points.Clear();

            double offsetX = _vmAtual.WorldX;
            double offsetY = _vmAtual.WorldY;

            foreach (var p in cabo.Vertices)
            {
                var local = new Point(p.X - offsetX, p.Y - offsetY);
                _polyline.Points.Add(local);
                _hitArea.Points.Add(local);
            }

            if (cabo.PreviewPonto.HasValue)
            {
                var preview = cabo.PreviewPonto.Value;
                var ultimo = cabo.Vertices.Count > 0 ? cabo.Vertices[^1] : preview;

                if (preview != ultimo)
                    AdicionarPonto(preview, offsetX, offsetY);
            }

            AtualizarHandlesVertices();
            InvalidateVisual();
        }

        private void AdicionarPonto(Point ponto, double offsetX, double offsetY)
        {
            var local = new Point(ponto.X - offsetX, ponto.Y - offsetY);
            _polyline.Points.Add(local);
            _hitArea.Points.Add(local);
        }

        private void AtualizarHandlesVertices()
        {
            if (_vmAtual == null)
                return;

            var cabo = _vmAtual.Cabo;
            int quantidade = ObterQuantidadeHandles();
            GarantirQuantidadeHandles(quantidade);

            double offsetX = _vmAtual.WorldX;
            double offsetY = _vmAtual.WorldY;

            for (int i = 0; i < _vertexHandles.Count; i++)
            {
                var handle = _vertexHandles[i];

                if (i >= quantidade)
                {
                    handle.Visibility = Visibility.Collapsed;
                    continue;
                }

                int indiceVertice = i + 1;
                Point ponto = cabo.Vertices[indiceVertice];
                Canvas.SetLeft(handle, ponto.X - offsetX - VertexHandleSize / 2);
                Canvas.SetTop(handle, ponto.Y - offsetY - VertexHandleSize / 2);
                handle.Visibility = DeveMostrarHandles() ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private int ObterQuantidadeHandles()
        {
            if (_vmAtual == null)
                return 0;

            var cabo = _vmAtual.Cabo;

            if (cabo.Vertices.Count <= 1)
                return 0;

            int limiteExclusivo = cabo.PreviewPonto.HasValue ? cabo.Vertices.Count : cabo.Vertices.Count - 1;
            return limiteExclusivo <= 1 ? 0 : limiteExclusivo - 1;
        }

        private void GarantirQuantidadeHandles(int quantidade)
        {
            while (_vertexHandles.Count < quantidade)
            {
                var handle = CriarHandleVertice();
                _vertexHandles.Add(handle);
                _root.Children.Add(handle);
            }
        }

        private static Ellipse CriarHandleVertice()
        {
            return new Ellipse
            {
                Width = VertexHandleSize,
                Height = VertexHandleSize,
                Fill = Brushes.DeepSkyBlue,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                IsHitTestVisible = false,
                Visibility = Visibility.Collapsed
            };
        }

        private bool DeveMostrarHandles()
        {
            return _vmAtual != null && !_vmAtual.IsPreview && (_vmAtual.IsSelecionado || _vmAtual.IsHover);
        }

        protected override void AplicarEstadoVisual(ElementoViewModel vm)
        {
            base.AplicarEstadoVisual(vm);

            if (vm is not CaboViewModel cabo)
                return;

            if (cabo.IsPreview)
            {
                _polyline.Stroke = Brushes.DeepSkyBlue;
                _polyline.StrokeThickness = 3;
                _polyline.Opacity = 0.65;
                _hitArea.IsHitTestVisible = false;
                AtualizarHandlesVertices();
                return;
            }

            _polyline.Stroke = cabo.Stroke;
            _polyline.StrokeThickness = cabo.StrokeThickness;
            _polyline.Opacity = 1;
            _hitArea.IsHitTestVisible = true;
            AtualizarHandlesVertices();
        }
    }
}