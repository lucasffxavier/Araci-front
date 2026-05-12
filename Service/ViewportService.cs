using Araci.ViewModels;

namespace Araci.Services
{
    public class ViewportService
    {
        // =========================
        // VIEWMODEL
        // =========================

        private readonly ViewportViewModel
            _viewportViewModel;

        // =========================
        // CONSTRUTOR
        // =========================

        public ViewportService(
            ViewportViewModel viewportViewModel)
        {
            _viewportViewModel =
                viewportViewModel;
        }

        // =========================
        // ADICIONAR
        // =========================

        public void AdicionarElemento(
            ElementoViewModel vm)
        {
            _viewportViewModel
                .Elementos
                .Add(vm);
        }

        // =========================
        // REMOVER
        // =========================

        public void RemoverElemento(
            ElementoViewModel vm)
        {
            _viewportViewModel
                .Elementos
                .Remove(vm);
        }

        // =========================
        // CABO
        // =========================

        public void AdicionarCabo(
            CaboViewModel vm)
        {
            AdicionarElemento(vm);
        }
    }
}