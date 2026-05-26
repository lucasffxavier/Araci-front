using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Applications.Editar.Base;
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
            CableVertexHandleLayer.RenderTransform = _cameraTransform;
            TerminalSnapLayer.RenderTransform = _cameraTransform;

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
            if (_context == null)
                return;

            if (_context.Navigation.TryHandleMiddleDoubleClick(e))
            {
                AtualizarCursorNavegacao();
                LiberarCapturaMouse();
                e.Handled = true;
                return;
            }

            if (_context.Navigation.TryBeginMiddlePan(e, this))
            {
                Focus();
                Keyboard.Focus(this);
                AtualizarCursorNavegacao();
                CaptureMouse();
                e.Handled = true;
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            if (_context.Navigation.TryBeginSpaceLeftPan(e, this))
            {
                Focus();
                Keyboard.Focus(this);
                AtualizarCursorNavegacao();
                CaptureMouse();
                e.Handled = true;
                return;
            }

            if (_context.Navigation.IsPanning)
            {
                e.Handled = true;
                return;
            }

            Focus();
            Keyboard.Focus(this);

            var vm = EncontrarElemento(e.OriginalSource as DependencyObject);
            Point worldPosition = GetWorldPos(e);
            ToolInputState inputState = CriarInputState(e, e.ChangedButton, e.ClickCount);

            _context.Input.MouseDown(vm, worldPosition, inputState);

            CaptureMouse();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_context?.Navigation.TryUpdatePan(e, this) == true)
            {
                AtualizarCursorNavegacao();
                e.Handled = true;
                return;
            }

            if (_context == null)
                return;

            Point worldPosition = GetWorldPos(e);
            _context.Input.MouseMove(worldPosition, CriarInputState(e));
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_context?.Navigation.TryEndMiddlePan(e) != true)
                return;

            AtualizarCursorNavegacao();
            LiberarCapturaMouse();
            e.Handled = true;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_context?.Navigation.TryEndSpaceLeftPan(e) == true)
            {
                AtualizarCursorNavegacao();
                LiberarCapturaMouse();
                e.Handled = true;
                return;
            }

            if (_context?.Navigation.ConsumeSuppressNextLeftButtonUp() == true)
            {
                e.Handled = true;
                return;
            }

            if (_context?.Navigation.IsPanning == true)
            {
                e.Handled = true;
                return;
            }

            if (_context != null)
            {
                Point worldPosition = GetWorldPos(e);
                _context.Input.MouseUp(worldPosition, CriarInputState(e, e.ChangedButton, e.ClickCount));
            }

            LiberarCapturaMouse();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_context?.Navigation.TryHandleMouseWheel(e, this) != true)
                return;

            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_context?.Navigation.TryHandleKeyDown(e) == true)
            {
                AtualizarCursorNavegacao();
                e.Handled = true;
                return;
            }

            if (_context == null)
                return;

            e.Handled = _context.Input.KeyDown(e.Key);

            if (e.Handled && e.Key == Key.Escape)
                LiberarCapturaMouse();
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_context?.Navigation.TryHandleKeyUp(e) != true)
                return;

            AtualizarCursorNavegacao();
            LiberarCapturaMouse();
            e.Handled = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _context?.Hover.Clear();
            _context?.TerminalSnap.Limpar();
            _context?.Navigation.CancelPan();
            AtualizarCursorNavegacao();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _context?.Navigation.Reset();
            LiberarCapturaMouse();
            Cursor = Cursors.Arrow;

            if (_context?.Viewport != null)
                _context.Viewport.Camera.PropertyChanged -= OnCameraChanged;
        }

        private void AtualizarCursorNavegacao()
        {
            if (_context?.Navigation.IsPanning == true || _context?.Navigation.IsSpacePressed == true)
                Cursor = Cursors.ScrollAll;
            else
                Cursor = Cursors.Arrow;
        }

        private void LiberarCapturaMouse()
        {
            if (IsMouseCaptured)
                ReleaseMouseCapture();
        }

        private ToolInputState CriarInputState(
            MouseEventArgs e,
            MouseButton? button = null,
            int clickCount = 0)
        {
            Point screenPosition = e.GetPosition(this);
            Point worldPosition = _context?.Viewport?.ScreenToWorld(screenPosition) ?? screenPosition;

            return new ToolInputState(
                Keyboard.Modifiers,
                button,
                clickCount,
                worldPosition,
                screenPosition);
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
