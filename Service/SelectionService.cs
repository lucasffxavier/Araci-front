using System.Collections.ObjectModel;
using System.Linq;

using Araci.ViewModels;

namespace Araci.Services
{
    public class SelectionService
    {
        private readonly EditorContext _context;

        public SelectionService(EditorContext context)
        {
            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
        }

        // =========================
        // SELECIONADOS
        // =========================

        public ObservableCollection<ElementoViewModel>
            Selecionados
        { get; }
            = new();

        // =========================
        // PRINCIPAL
        // =========================

        public ElementoViewModel?
            Principal =>
                Selecionados.FirstOrDefault();

        // =========================
        // SELECIONAR
        // =========================

        public void Selecionar(
            ElementoViewModel vm,
            bool adicionar = false)
        {
            if (vm == null)
                return;

            // =========================
            // LIMPA SE NÃO FOR MULTISELECT
            // =========================

            if (!adicionar)
            {
                Limpar();
            }

            // =========================
            // EVITA DUPLICIDADE
            // =========================

            if (Selecionados.Contains(vm))
            {
                vm.IsSelecionado = true;

                _context.Editor
                    .ElementoSelecionado = vm;

                return;
            }

            // =========================
            // MARCA VISUALMENTE
            // =========================

            vm.IsSelecionado = true;

            Selecionados.Add(vm);

            _context.Editor
                .ElementoSelecionado = vm;
        }

        // =========================
        // DESELECIONAR
        // =========================

        public void Deselecionar(
            ElementoViewModel vm)
        {
            if (vm == null)
                return;

            if (!Selecionados.Contains(vm))
            {
                vm.IsSelecionado = false;
                return;
            }

            vm.IsSelecionado = false;

            Selecionados.Remove(vm);

            _context.Editor
                .ElementoSelecionado =
                    Principal;
        }

        // =========================
        // TOGGLE
        // =========================

        public void Toggle(
            ElementoViewModel vm)
        {
            if (vm == null)
                return;

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

        public void Limpar()
        {
            foreach (var item in
                     Selecionados.ToList())
            {
                item.IsSelecionado = false;
            }

            Selecionados.Clear();

            _context.Editor
                .ElementoSelecionado = null;
        }

        // =========================
        // SINCRONIZAÇÃO
        // =========================

        public void GarantirConsistencia()
        {
            foreach (var item in
                     Selecionados.ToList())
            {
                if (item == null)
                    continue;

                item.IsSelecionado = true;
            }
        }
    }
}
