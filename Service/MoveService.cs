using System;
using System.Windows;

using Araci.ViewModels;

namespace Araci.Services
{
    public static class MoveService
    {
        // margem visual para stroke/borda/render
        private const double MARGEM_VISUAL = 2;

        public static void Mover(
            ElementoViewModel vm,
            Vector delta)
        {
            double maxX =
                AppServices.Viewport?.Largura ?? 1000;

            double maxY =
                AppServices.Viewport?.Altura ?? 800;

            double largura =
                vm.Largura;

            double altura =
                vm.Altura;

            double novoX =
                vm.X + delta.X;

            double novoY =
                vm.Y + delta.Y;

            double limiteDireito =
                Math.Max(0,
                    maxX - largura - MARGEM_VISUAL);

            double limiteInferior =
                Math.Max(0,
                    maxY - altura - MARGEM_VISUAL);

            novoX =
                Math.Max(0,
                    Math.Min(novoX, limiteDireito));

            novoY =
                Math.Max(0,
                    Math.Min(novoY, limiteInferior));

            double dx =
                novoX - vm.X;

            double dy =
                novoY - vm.Y;

            vm.X = novoX;
            vm.Y = novoY;

            if (vm is CaboViewModel cabo)
            {
                cabo.X2 += dx;
                cabo.Y2 += dy;
            }
        }
    }
}