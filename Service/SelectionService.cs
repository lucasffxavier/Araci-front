using System.Collections.ObjectModel;
using System.Linq;

using Araci.ViewModels;

namespace Araci.Services
{
    public static class SelectionService
    {
        public static ObservableCollection<ElementoViewModel>
            Selecionados
        { get; }
            = new();

        public static ElementoViewModel? Principal =>
            Selecionados.FirstOrDefault();

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
            vm.Modelo.Selecionado = true;

            Selecionados.Add(vm);

            AppServices.Editor.ElementoSelecionado = vm;
        }

        public static void Deselecionar(
            ElementoViewModel vm)
        {
            if (vm == null)
                return;

            if (!Selecionados.Contains(vm))
                return;

            vm.IsSelecionado = false;
            vm.Modelo.Selecionado = false;

            Selecionados.Remove(vm);

            AppServices.Editor.ElementoSelecionado =
                Principal;
        }

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

        public static void Limpar()
        {
            foreach (var item in Selecionados.ToList())
            {
                item.IsSelecionado = false;
                item.Modelo.Selecionado = false;
            }

            Selecionados.Clear();

            AppServices.Editor.ElementoSelecionado = null;
        }
    }
}