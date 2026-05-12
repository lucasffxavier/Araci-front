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
        private readonly ViewportViewModel
            _viewportViewModel;

        public ViewportView()
        {
            InitializeComponent();

            // =========================
            // DOCUMENTO CENTRAL
            // =========================

            if (AppServices.Document == null)
            {
                AppServices.Document =
                    new Core.Documents.AraciDocument();
            }

            // =========================
            // VIEWMODEL
            // =========================

            _viewportViewModel =
                new ViewportViewModel(
                    AppServices.Document);

            DataContext =
                _viewportViewModel;

            // =========================
            // VIEWPORT
            // =========================

            AppServices.Viewport =
                new ViewportService(
                    _viewportViewModel);

            AppServices.ViewportReference =
                this;
        }

        // =========================
        // LOADED
        // =========================

        private void OnLoaded(
            object sender,
            RoutedEventArgs e)
        {
            Focus();

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
            if (AppServices.Viewport == null)
                return;

            double larguraReal =
                ActualWidth;

            double alturaReal =
                ActualHeight;

            if (RootBorder != null)
            {
                larguraReal -=
                    RootBorder.BorderThickness.Left +
                    RootBorder.BorderThickness.Right;

                alturaReal -=
                    RootBorder.BorderThickness.Top +
                    RootBorder.BorderThickness.Bottom;
            }

            larguraReal =
                Math.Max(0, larguraReal);

            alturaReal =
                Math.Max(0, alturaReal);

            AppServices.Viewport.AtualizarTamanho(
                new Size(
                    larguraReal,
                    alturaReal));
        }

        // =========================
        // POSIÇÃO
        // =========================

        private Point GetPos(
            MouseEventArgs e)
        {
            return e.GetPosition(this);
        }

        // =========================
        // MOUSE DOWN
        // =========================

        private void OnPreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            Focus();

            ElementoViewModel? vm = null;

            DependencyObject? origem =
                e.OriginalSource
                    as DependencyObject;

            while (origem != null)
            {
                if (origem is FrameworkElement fe &&
                    fe.DataContext is ElementoViewModel elemento)
                {
                    vm = elemento;
                    break;
                }

                origem =
                    VisualTreeHelper
                        .GetParent(origem);
            }

            var pos =
                GetPos(e);

            AppServices.Tools.HandleMouseDown(
                vm,
                pos);
        }

        // =========================
        // MOUSE MOVE
        // =========================

        private void OnPreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            var pos =
                GetPos(e);

            AppServices.Tools.HandleMouseMove(
                pos);
        }

        // =========================
        // MOUSE UP
        // =========================

        private void OnPreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            var pos =
                GetPos(e);

            AppServices.Tools.HandleMouseUp(
                pos);
        }

        // =========================
        // KEYBOARD
        // =========================

        private void OnPreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            AppServices.Tools.HandleKeyDown(
                e.Key);

            if (e.Key == Key.Escape)
            {
                AppServices.Tools
                    .VoltarParaSelecao();

                SelectionService.Limpar();

                e.Handled = true;
            }
        }
    }
}