    using System;
    using System.Windows;
    using Araci.ViewModels;

    namespace Araci.Services
    {
        public static class MoveService
        {
            public static event Action<double, double>? DeltaChanged;

            public static void Mover(ElementoViewModel vm, Vector delta)
            {
                vm.X += delta.X;
                vm.Y += delta.Y;

                AtualizarHUD(delta);
            }

            public static void MoverCabo(CaboViewModel vm, Vector delta)
            {
                vm.X += delta.X;
                vm.Y += delta.Y;
                vm.X2 += delta.X;
                vm.Y2 += delta.Y;

                AtualizarHUD(delta);
            }

            private static void AtualizarHUD(Vector delta)
            {
                var hud = AppServices.MoveHud;

                hud.DeltaX += delta.X;
                hud.DeltaY += delta.Y;

                DeltaChanged?.Invoke(delta.X, delta.Y);
            }
        }
    }