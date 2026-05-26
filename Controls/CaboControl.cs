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
        private readonly Canvas _root;
        private readonly Polyline _polyline;
        private readonly Polyline _hitArea;

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
                {
                    var local = new Point(preview.X - offsetX, preview.Y - offsetY);

                    _polyline.Points.Add(local);
                    _hitArea.Points.Add(local);
                }
            }

            InvalidateVisual();
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
                return;
            }

            _polyline.Stroke = cabo.Stroke;
            _polyline.StrokeThickness = cabo.StrokeThickness;
            _polyline.Opacity = 1;
            _hitArea.IsHitTestVisible = true;
        }
    }
}
