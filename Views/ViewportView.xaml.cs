using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Views
{
    public partial class ViewportView : UserControl
    {
        private readonly MatrixTransform _cameraTransform = new();

        private ViewportViewModel? _viewportViewModel;
        private EditorContext? _context;

        public ViewportView()
        {
            InitializeComponent();
        }

        public void Inicializar(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _viewportViewModel = new ViewportViewModel(_context);

            DataContext = _viewportViewModel;

            _context.InicializarViewport(_viewportViewModel);

            ConfigurarCamera();

            Unloaded += OnUnloaded;
        }

        private void ConfigurarCamera()
        {
            if (_context?.Viewport == null)
                return;

            WorldLayer.RenderTransform = _cameraTransform;
            SelectionLayer.RenderTransform = _cameraTransform;

            _context.Viewport.Camera.PropertyChanged += OnCameraChanged;

            AtualizarCameraTransform();
        }

        private void OnCameraChanged(object? sender, PropertyChangedEventArgs e)
        {
            AtualizarCameraTransform();
        }

        private void AtualizarCameraTransform()
        {
            if (_context?.Viewport == null)
                return;

            var camera = _context.Viewport.Camera;

            _cameraTransform.Matrix = new Matrix(
                camera.Zoom,
                0,
                0,
                camera.Zoom,
                camera.Offset.X,
                camera.Offset.Y);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);

            AtualizarViewport();

            SizeChanged += (_, _) => AtualizarViewport();
        }

        private void AtualizarViewport()
        {
            if (_context?.Viewport == null)
                return;

            double largura = ActualWidth;
            double altura = ActualHeight;

            if (RootBorder != null)
            {
                largura -= RootBorder.BorderThickness.Left + RootBorder.BorderThickness.Right;
                altura -= RootBorder.BorderThickness.Top + RootBorder.BorderThickness.Bottom;
            }

            largura = Math.Max(0, largura);
            altura = Math.Max(0, altura);

            _context.Viewport.AtualizarTamanho(new Size(largura, altura));
        }

        private Point GetWorldPos(MouseEventArgs e)
        {
            Point screen = e.GetPosition(this);

            return _context?.Viewport?.ScreenToWorld(screen) ?? screen;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            Focus();
            Keyboard.Focus(this);

            var vm = EncontrarElemento(e.OriginalSource as DependencyObject);

            _context.Input.MouseDown(vm, GetWorldPos(e));

            CaptureMouse();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            _context?.Input.MouseMove(GetWorldPos(e));
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _context?.Input.MouseUp(GetWorldPos(e));

            if (IsMouseCaptured)
                ReleaseMouseCapture();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_context?.Viewport == null)
                return;

            var camera = _context.Viewport.Camera;

            Point cursor = e.GetPosition(this);
            Point worldBefore = camera.ScreenToWorld(cursor);

            double factor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            camera.Zoom = Math.Max(0.1, Math.Min(8, camera.Zoom * factor));

            camera.Offset = new Point(
                cursor.X - worldBefore.X * camera.Zoom,
                cursor.Y - worldBefore.Y * camera.Zoom);

            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_context == null)
                return;

            e.Handled = _context.Input.KeyDown(e.Key);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_context?.Viewport != null)
                _context.Viewport.Camera.PropertyChanged -= OnCameraChanged;
        }

        private static ElementoViewModel? EncontrarElemento(DependencyObject? origem)
        {
            while (origem != null)
            {
                if (origem is FrameworkElement fe && fe.DataContext is ElementoViewModel vm)
                    return vm;

                origem = VisualTreeHelper.GetParent(origem);
            }

            return null;
        }
    }
}