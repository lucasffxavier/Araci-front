using System;
using System.Windows;

using Araci.ViewModels;

namespace Araci.Services
{
    public static class MoveService
    {
        private const double
            MARGEM_VISUAL = 2;

        public static void MoverVisual(
            ElementoViewModel vm,
            Vector delta)
        {
            double maxX =
                AppServices.Viewport?.Largura
                    ?? 1000;

            double maxY =
                AppServices.Viewport?.Altura
                    ?? 800;

            Rect bounds =
                vm.Bounds;

            double novoX =
                bounds.X + delta.X;

            double novoY =
                bounds.Y + delta.Y;

            double limiteDireito =
                Math.Max(
                    0,
                    maxX - bounds.Width - MARGEM_VISUAL);

            double limiteInferior =
                Math.Max(
                    0,
                    maxY - bounds.Height - MARGEM_VISUAL);

            novoX =
                Math.Max(
                    0,
                    Math.Min(
                        novoX,
                        limiteDireito));

            novoY =
                Math.Max(
                    0,
                    Math.Min(
                        novoY,
                        limiteInferior));

            vm.X = novoX;
            vm.Y = novoY;
        }
    }
}