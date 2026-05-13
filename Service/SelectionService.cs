using System.Collections.ObjectModel;
using System.Linq;

using Araci.ViewModels;

namespace Araci.Services
{
    public static class SelectionService
    {
        // =========================
        // SELECIONADOS
        // =========================

        public static ObservableCollection<ElementoViewModel>
            Selecionados
        { get; }
            = new();

        // =========================
        // PRINCIPAL
        // =========================

        public static ElementoViewModel?
            Principal =>
                Selecionados.FirstOrDefault();

        // =========================
        // SELECIONAR
        // =========================

        public static void Selecionar(
            ElementoViewModel vm,
            bool adicionar = false)
        {
            if (vm == null)
                return;

            if (!adicionar)
            {
                Limpar();
            }

            if (Selecionados.Contains(vm))
                return;

            vm.IsSelecionado = true;

            Selecionados.Add(vm);

            AppServices.Editor
                .ElementoSelecionado = vm;
        }

        // =========================
        // DESELECIONAR
        // =========================

        public static void Deselecionar(
            ElementoViewModel vm)
        {
            if (vm == null)
                return;

            if (!Selecionados.Contains(vm))
                return;

            vm.IsSelecionado = false;

            Selecionados.Remove(vm);

            AppServices.Editor
                .ElementoSelecionado =
                    Principal;
        }

        // =========================
        // TOGGLE
        // =========================

        public static void Toggle(
            ElementoViewModel vm)
        {
            if (Selecionados.Contains(vm))
            {
                Deselecionar(vm);
            }
            else
            {
                Selecionar(vm, true);
            }
        }

        // =========================
        // LIMPAR
        // =========================

        public static void Limpar()
        {
            foreach (var item in
                     Selecionados.ToList())
            {
                item.IsSelecionado = false;
            }

            Selecionados.Clear();

            AppServices.Editor
                .ElementoSelecionado = null;
        }
    }
}