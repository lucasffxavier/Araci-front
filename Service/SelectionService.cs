using System.Linq;
using System.Windows.Input;

using Araci.ViewModels;

namespace Araci.Services
{
    public static class SelectionService
    {
        // =========================
        // SELECIONAR
        // =========================

        public static void Selecionar(
            ElementoViewModel vm)
        {
            bool ctrl =
                Keyboard.Modifiers.HasFlag(
                    ModifierKeys.Control);

            if (!ctrl)
            {
                Limpar();
            }

            Toggle(vm, ctrl);
        }

        // =========================
        // TOGGLE
        // =========================

        private static void Toggle(
            ElementoViewModel vm,
            bool ctrl)
        {
            var selecionados =
                AppServices
                    .Editor
                    .ElementosSelecionados;

            // =========================
            // CTRL + CLICK
            // =========================

            if (ctrl)
            {
                if (selecionados.Contains(vm))
                {
                    vm.IsSelecionado = false;

                    selecionados.Remove(vm);
                }
                else
                {
                    vm.IsSelecionado = true;

                    selecionados.Add(vm);
                }
            }
            else
            {
                vm.IsSelecionado = true;

                if (!selecionados.Contains(vm))
                {
                    selecionados.Add(vm);
                }
            }

            AtualizarPrincipal();
        }

        // =========================
        // LIMPAR
        // =========================

        public static void Limpar()
        {
            foreach (var item in AppServices
                         .Editor
                         .ElementosSelecionados
                         .ToList())
            {
                item.IsSelecionado = false;
            }

            AppServices
                .Editor
                .ElementosSelecionados
                .Clear();

            AtualizarPrincipal();
        }

        // =========================
        // PRINCIPAL
        // =========================

        private static void AtualizarPrincipal()
        {
            AppServices.Editor.ElementoSelecionado =
                AppServices.Editor
                    .ElementosSelecionados
                    .LastOrDefault();

            AppServices
                .Editor
                .NotifySelecaoAlterada();
        }
    }
}