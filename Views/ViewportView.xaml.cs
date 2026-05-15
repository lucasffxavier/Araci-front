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
        private readonly ViewportViewModel _viewportViewModel;

        private readonly EditorContext _context;

        private readonly MatrixTransform _cameraTransform =
            new();

        public ViewportView()
        {
            InitializeComponent();

            _context =
                AppServices.Current;

            if (_context.Document == null)
            {
                _context.Document =
                    new Core.Documents.AraciDocument();
            }

            _viewportViewModel =
                new ViewportViewModel(
                    _context);

            DataContext = _viewportViewModel;

            _context.Viewport =
                new ViewportService(
                    _viewportViewModel);

            ConfigurarCamera();

            Unloaded += OnUnloaded;
        }

        // =========================
        // CAMERA
        // =========================

        private void ConfigurarCamera()
        {
            if (_context.Viewport == null)
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
            if (_context.Viewport != null)
            {
                _context.Viewport.Camera.PropertyChanged -=
                    OnCameraChanged;
            }
        }

        private void AtualizarCameraTransform()
        {
            if (_context.Viewport == null)
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

        // =========================
        // LOADED
        // =========================

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();

            Keyboard.Focus(this);

            AtualizarViewport();

            SizeChanged += (_, __) =>
            {
                AtualizarViewport();
            };
        }

        // =========================
        // VIEWPORT SIZE
        // =========================

        private void AtualizarViewport()
        {
            if (_context.Viewport == null)
                return;

            double larguraReal = ActualWidth;

            double alturaReal = ActualHeight;

            if (RootBorder != null)
            {
                larguraReal -=
                    RootBorder.BorderThickness.Left +
                    RootBorder.BorderThickness.Right;

                alturaReal -=
                    RootBorder.BorderThickness.Top +
                    RootBorder.BorderThickness.Bottom;
            }

            larguraReal = Math.Max(0, larguraReal);

            alturaReal = Math.Max(0, alturaReal);

            _context.Viewport.AtualizarTamanho(
                new Size(
                    larguraReal,
                    alturaReal));
        }

        // =========================
        // POSIÇÃO
        // =========================

        private Point GetPos(MouseEventArgs e)
        {
            return e.GetPosition(this);
        }

        // =========================
        // MOUSE DOWN
        // =========================

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            Keyboard.Focus(this);

            ElementoViewModel? vm = null;

            DependencyObject? origem =
                e.OriginalSource as DependencyObject;

            while (origem != null)
            {
                if (origem is FrameworkElement fe &&
                    fe.DataContext is ElementoViewModel elemento)
                {
                    vm = elemento;

                    break;
                }

                origem =
                    VisualTreeHelper.GetParent(origem);
            }

            var pos = GetPos(e);

            _context.Input.MouseDown(
                vm,
                pos);

            CaptureMouse();
        }

        // =========================
        // MOUSE MOVE
        // =========================

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var pos = GetPos(e);

            _context.Input.MouseMove(pos);
        }

        // =========================
        // MOUSE UP
        // =========================

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = GetPos(e);

            _context.Input.MouseUp(pos);

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }

        // =========================
        // ZOOM
        // =========================

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_context.Viewport == null)
                return;

            var camera =
                _context.Viewport.Camera;

            Point cursor =
                GetPos(e);

            Point worldBefore =
                camera.ScreenToWorld(cursor);

            double factor =
                e.Delta > 0
                    ? 1.1
                    : 1 / 1.1;

            camera.Zoom =
                Math.Max(
                    0.1,
                    Math.Min(
                        8,
                        camera.Zoom * factor));

            camera.Offset =
                new Point(
                    cursor.X - worldBefore.X * camera.Zoom,
                    cursor.Y - worldBefore.Y * camera.Zoom);

            e.Handled = true;
        }

        // =========================
        // KEYBOARD
        // =========================

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled =
                _context.Input.KeyDown(e.Key);
        }
    }
}
