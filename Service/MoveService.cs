using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class MoveService
    {
        public static event Action<double, double>? DeltaChanged;

        // =========================
        // ELEMENTOS NORMAIS
        // =========================

        public static void Mover(
            FrameworkElement elemento,
            ElementoViewModel vm,
            Vector delta)
        {
            if (!TryGetCanvas(elemento, out var canvas))
                return;

            double largura = elemento.ActualWidth;
            double altura = elemento.ActualHeight;

            double novoX = vm.X + delta.X;
            double novoY = vm.Y + delta.Y;

            novoX = Math.Max(0,
                Math.Min(novoX, canvas.ActualWidth - largura));

            novoY = Math.Max(0,
                Math.Min(novoY, canvas.ActualHeight - altura));

            vm.X = novoX;
            vm.Y = novoY;

            DeltaChanged?.Invoke(delta.X, delta.Y);
        }

        // =========================
        // CABO
        // =========================

        public static void MoverCabo(
            FrameworkElement elemento,
            CaboViewModel vm,
            Vector delta)
        {
            if (!TryGetCanvas(elemento, out var canvas))
                return;

            double novoX1 = vm.X + delta.X;
            double novoY1 = vm.Y + delta.Y;
            double novoX2 = vm.X2 + delta.X;
            double novoY2 = vm.Y2 + delta.Y;

            double minX = Math.Min(novoX1, novoX2);
            double minY = Math.Min(novoY1, novoY2);
            double maxX = Math.Max(novoX1, novoX2);
            double maxY = Math.Max(novoY1, novoY2);

            if (minX < 0) delta.X -= minX;
            if (minY < 0) delta.Y -= minY;

            if (maxX > canvas.ActualWidth)
                delta.X -= (maxX - canvas.ActualWidth);

            if (maxY > canvas.ActualHeight)
                delta.Y -= (maxY - canvas.ActualHeight);

            vm.X += delta.X;
            vm.Y += delta.Y;
            vm.X2 += delta.X;
            vm.Y2 += delta.Y;

            DeltaChanged?.Invoke(delta.X, delta.Y);
        }

        // =========================
        // UTIL
        // =========================

        private static bool TryGetCanvas(
            FrameworkElement elemento,
            out Canvas canvas)
        {
            canvas = null!;

            if (VisualTreeHelper.GetParent(elemento) is not ContentPresenter presenter)
                return false;

            if (VisualTreeHelper.GetParent(presenter) is not Canvas c)
                return false;

            canvas = c;
            return true;
        }
    }
}