using Araci.Services;
using Araci.ViewModels;

using System;
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
                    _context.Document,
                    _context.Scene);

            DataContext = _viewportViewModel;

            _context.Viewport =
                new ViewportService(
                    _viewportViewModel);

            _context.ViewportReference = this;
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
