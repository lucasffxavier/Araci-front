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
        private bool _isPanning;
        private bool _isSpacePressed;
        private bool _isSpaceLeftPanning;
        private bool _suppressNextLeftButtonUp;
        private Point _lastPanPoint;

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

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_context?.Viewport == null || e.ChangedButton != MouseButton.Middle)
                return;

            if (e.ClickCount >= 2)
            {
                CancelarPan();
                _context.Viewport.ZoomExtents();
                e.Handled = true;
                return;
            }

            IniciarPan(e.GetPosition(this), spaceLeftPan: false);
            e.Handled = true;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_context?.Viewport != null && _isSpacePressed)
            {
                IniciarPan(e.GetPosition(this), spaceLeftPan: true);
                e.Handled = true;
                return;
            }

            if (_context == null || _isPanning)
                return;

            Focus();
            Keyboard.Focus(this);

            var vm = EncontrarElemento(e.OriginalSource as DependencyObject);

            _context.Input.MouseDown(vm, GetWorldPos(e));

            CaptureMouse();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                AtualizarPan(e.GetPosition(this));
                e.Handled = true;
                return;
            }

            _context?.Input.MouseMove(GetWorldPos(e));
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle || !_isPanning)
                return;

            CancelarPan();
            e.Handled = true;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isSpaceLeftPanning)
            {
                CancelarPan();
                e.Handled = true;
                return;
            }

            if (_suppressNextLeftButtonUp)
            {
                _suppressNextLeftButtonUp = false;
                e.Handled = true;
                return;
            }

            if (_isPanning)
                return;

            _context?.Input.MouseUp(GetWorldPos(e));

            if (IsMouseCaptured)
                ReleaseMouseCapture();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_context?.Viewport == null)
                return;

            Point cursor = e.GetPosition(this);

            if (e.Delta > 0)
                _context.Viewport.ZoomInAt(cursor);
            else
                _context.Viewport.ZoomOutAt(cursor);

            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _isSpacePressed = true;
                Cursor = Cursors.ScrollAll;
                e.Handled = true;
                return;
            }

            if (TryHandleViewportShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (_context == null)
                return;

            e.Handled = _context.Input.KeyDown(e.Key);
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space)
                return;

            _isSpacePressed = false;

            if (_isSpaceLeftPanning)
            {
                _suppressNextLeftButtonUp = true;
                CancelarPan();
            }
            else
            {
                Cursor = Cursors.Arrow;
            }

            e.Handled = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_isPanning)
                CancelarPan();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isSpacePressed = false;

            if (_isPanning)
                CancelarPan();

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            Cursor = Cursors.Arrow;

            if (_context?.Viewport != null)
                _context.Viewport.Camera.PropertyChanged -= OnCameraChanged;
        }

        private bool TryHandleViewportShortcut(KeyEventArgs e)
        {
            if (_context?.Viewport == null)
                return false;

            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                return false;

            switch (e.Key)
            {
                case Key.OemPlus:
                case Key.Add:
                    _context.Viewport.ZoomInAtCenter();
                    return true;

                case Key.OemMinus:
                case Key.Subtract:
                    _context.Viewport.ZoomOutAtCenter();
                    return true;

                case Key.D0:
                case Key.NumPad0:
                    _context.Viewport.ResetCamera();
                    return true;

                case Key.D1:
                case Key.NumPad1:
                    _context.Viewport.Zoom100AtCenter();
                    return true;

                default:
                    return false;
            }
        }

        private void IniciarPan(Point start, bool spaceLeftPan)
        {
            Focus();
            Keyboard.Focus(this);

            _isPanning = true;
            _isSpaceLeftPanning = spaceLeftPan;
            _lastPanPoint = start;
            Cursor = Cursors.ScrollAll;
            CaptureMouse();
        }

        private void AtualizarPan(Point current)
        {
            if (_context?.Viewport == null)
                return;

            Vector delta = current - _lastPanPoint;

            _context.Viewport.Pan(delta);

            _lastPanPoint = current;
        }

        private void CancelarPan()
        {
            _isPanning = false;
            _isSpaceLeftPanning = false;
            Cursor = _isSpacePressed ? Cursors.ScrollAll : Cursors.Arrow;

            if (IsMouseCaptured)
                ReleaseMouseCapture();
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
