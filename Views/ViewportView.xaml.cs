using Araci.Services;
using Araci.ViewModels;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Araci.Views
{
    public partial class ViewportView : UserControl
    {
        private ViewportViewModel? _viewportViewModel;

        private EditorContext? _context;

        private readonly MatrixTransform
            _cameraTransform = new();

        public ViewportView()
        {
            InitializeComponent();
        }

        public void Inicializar(
            EditorContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));

            _viewportViewModel =
                new ViewportViewModel(_context);

            DataContext = _viewportViewModel;

            _context.InicializarViewport(
                _viewportViewModel);

            ConfigurarCamera();

            Unloaded += OnUnloaded;
        }

        private void ConfigurarCamera()
        {
            if (_context?.Viewport == null)
                return;

            WorldLayer.RenderTransform =
                _cameraTransform;

            SelectionLayer.RenderTransform =
                _cameraTransform;

            _context.Viewport.Camera.PropertyChanged +=
                OnCameraChanged;

            AtualizarCameraTransform();
        }

        private void OnCameraChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            AtualizarCameraTransform();
        }

        private void OnUnloaded(
            object sender,
            RoutedEventArgs e)
        {
            if (_context?.Viewport != null)
            {
                _context.Viewport.Camera.PropertyChanged -=
                    OnCameraChanged;
            }
        }

        private void AtualizarCameraTransform()
        {
            if (_context?.Viewport == null)
                return;

            var camera =
                _context.Viewport.Camera;

            _cameraTransform.Matrix =
                new Matrix(
                    camera.Zoom,
                    0,
                    0,
                    camera.Zoom,
                    camera.Offset.X,
                    camera.Offset.Y);
        }

        private void OnLoaded(
            object sender,
            RoutedEventArgs e)
        {
            Focus();

            Keyboard.Focus(this);

            AtualizarViewport();

            SizeChanged += (_, __) =>
                AtualizarViewport();
        }

        private void AtualizarViewport()
        {
            if (_context?.Viewport == null)
                return;

            double largura = ActualWidth;
            double altura = ActualHeight;

            if (RootBorder != null)
            {
                largura -= RootBorder.BorderThickness.Left
                         + RootBorder.BorderThickness.Right;

                altura -= RootBorder.BorderThickness.Top
                        + RootBorder.BorderThickness.Bottom;
            }

            largura = Math.Max(0, largura);
            altura = Math.Max(0, altura);

            _context.Viewport.AtualizarTamanho(
                new Size(largura, altura));
        }

        private Point GetPos(MouseEventArgs e) =>
            e.GetPosition(this);

        private void OnPreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            Focus();

            Keyboard.Focus(this);

            ElementoViewModel? vm = null;

            DependencyObject? origem =
                e.OriginalSource as DependencyObject;

            while (origem != null)
            {
                if (origem is FrameworkElement fe &&
                    fe.DataContext is ElementoViewModel el)
                {
                    vm = el;
                    break;
                }

                origem = VisualTreeHelper.GetParent(origem);
            }

            _context.Input.MouseDown(vm, GetPos(e));

            CaptureMouse();
        }

        private void OnPreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            _context?.Input.MouseMove(GetPos(e));
        }

        private void OnPreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            _context?.Input.MouseUp(GetPos(e));

            if (IsMouseCaptured)
                ReleaseMouseCapture();
        }

        private void OnPreviewMouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            if (_context?.Viewport == null)
                return;

            var camera =
                _context.Viewport.Camera;

            Point cursor = GetPos(e);

            Point worldBefore =
                camera.ScreenToWorld(cursor);

            double factor =
                e.Delta > 0 ? 1.1 : 1 / 1.1;

            camera.Zoom = Math.Max(
                0.1,
                Math.Min(8, camera.Zoom * factor));

            camera.Offset = new Point(
                cursor.X - worldBefore.X * camera.Zoom,
                cursor.Y - worldBefore.Y * camera.Zoom);

            e.Handled = true;
        }

        private void OnPreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (_context != null)
            {
                e.Handled =
                    _context.Input.KeyDown(e.Key);
            }
        }
    }
}