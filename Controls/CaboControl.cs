using System.Collections.Specialized;
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

        // Área de hit test
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

            vm.Cabo.Vertices.CollectionChanged += OnVerticesChanged;

            AtualizarPolyline();
        }

        private void Desconectar()
        {
            if (_vmAtual == null)
                return;

            _vmAtual.Cabo.Vertices.CollectionChanged -= OnVerticesChanged;

            _vmAtual = null;
        }

        private void OnVerticesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            AtualizarPolyline();
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

            // 🔥 ORDEM CORRETA:
            // 1. VÉRTICES
            foreach (var p in cabo.Vertices)
            {
                var local = new Point(
                    p.X - offsetX,
                    p.Y - offsetY);

                _polyline.Points.Add(local);
                _hitArea.Points.Add(local);
            }

            // 2. PREVIEW (EXTENSÃO FINAL)
            if (cabo.PreviewPonto.HasValue)
            {
                var p = cabo.PreviewPonto.Value;

                var local = new Point(
                    p.X - offsetX,
                    p.Y - offsetY);

                _polyline.Points.Add(local);
                _hitArea.Points.Add(local);
            }
        }

        protected override void AplicarEstadoVisual(ElementoViewModel vm)
        {
            base.AplicarEstadoVisual(vm);

            if (vm is not CaboViewModel cabo)
                return;

            _polyline.Stroke = cabo.Stroke;
            _polyline.StrokeThickness = cabo.StrokeThickness;
        }
    }
}