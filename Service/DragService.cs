using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Araci.ViewModels;

namespace Araci.Services
{
    public class DragService
    {
        // =========================
        // CAMPOS
        // =========================

        private readonly UIElement _elemento;

        private bool _arrastando;

        private Point _ultimoPonto;

        // =========================
        // CONSTRUTOR
        // =========================

        public DragService(
            UIElement elemento)
        {
            _elemento = elemento;

            _elemento.MouseLeftButtonDown +=
                MouseDown;

            _elemento.MouseMove +=
                MouseMove;

            _elemento.MouseLeftButtonUp +=
                MouseUp;
        }

        // =========================
        // MOUSE DOWN
        // =========================

        private void MouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!AppServices
                .Tools
                .FerramentaAtual
                .PermiteArrastar)
            {
                return;
            }

            if (_elemento is not FrameworkElement fe)
                return;

            if (fe.DataContext is not ElementoViewModel vm)
                return;

            if (!vm.IsSelecionado)
                return;

            if (VisualTreeHelper.GetParent(fe)
                is not ContentPresenter presenter)
                return;

            if (VisualTreeHelper.GetParent(presenter)
                is not Canvas canvas)
                return;

            _arrastando = true;

            _ultimoPonto =
                e.GetPosition(canvas);

            _elemento.CaptureMouse();

            e.Handled = true;
        }

        // =========================
        // MOUSE MOVE
        // =========================

        private void MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (!_arrastando)
                return;

            if (_elemento is not FrameworkElement fe)
                return;

            if (fe.DataContext is not ElementoViewModel vm)
                return;

            if (VisualTreeHelper.GetParent(fe)
                is not ContentPresenter presenter)
                return;

            if (VisualTreeHelper.GetParent(presenter)
                is not Canvas canvas)
                return;

            Point atual =
                e.GetPosition(canvas);

            double dx =
                atual.X - _ultimoPonto.X;

            double dy =
                atual.Y - _ultimoPonto.Y;

            // ====================================
            // CABO
            // ====================================

            if (vm is CaboViewModel cabo)
            {
                double novoX1 =
                    cabo.X + dx;

                double novoY1 =
                    cabo.Y + dy;

                double novoX2 =
                    cabo.X2 + dx;

                double novoY2 =
                    cabo.Y2 + dy;

                double minX =
                    Math.Min(novoX1, novoX2);

                double minY =
                    Math.Min(novoY1, novoY2);

                double maxX =
                    Math.Max(novoX1, novoX2);

                double maxY =
                    Math.Max(novoY1, novoY2);

                if (minX < 0)
                    dx -= minX;

                if (minY < 0)
                    dy -= minY;

                if (maxX > canvas.ActualWidth)
                    dx -=
                        maxX - canvas.ActualWidth;

                if (maxY > canvas.ActualHeight)
                    dy -=
                        maxY - canvas.ActualHeight;

                cabo.X += dx;
                cabo.Y += dy;

                cabo.X2 += dx;
                cabo.Y2 += dy;
            }

            // ====================================
            // ELEMENTOS NORMAIS
            // ====================================

            else
            {
                double largura =
                    fe.ActualWidth;

                double altura =
                    fe.ActualHeight;

                double novoX =
                    vm.X + dx;

                double novoY =
                    vm.Y + dy;

                novoX =
                    Math.Max(
                        0,
                        Math.Min(
                            novoX,
                            canvas.ActualWidth
                            - largura));

                novoY =
                    Math.Max(
                        0,
                        Math.Min(
                            novoY,
                            canvas.ActualHeight
                            - altura));

                vm.X = novoX;
                vm.Y = novoY;
            }

            _ultimoPonto =
                atual;
        }

        // =========================
        // MOUSE UP
        // =========================

        private void MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            _arrastando = false;

            _elemento.ReleaseMouseCapture();
        }
    }
}