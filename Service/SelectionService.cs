using Araci.ViewModels;

namespace Araci.Services
{
    public static class SelectionService
    {
        // =========================
        // ELEMENTO ATUAL
        // =========================

        private static ElementoViewModel?
            _selecionado;

        // =========================
        // SELECIONAR
        // =========================

        public static void Selecionar(
            ElementoViewModel vm)
        {
            // =========================
            // REMOVE ANTERIOR
            // =========================

            if (_selecionado != null)
            {
                _selecionado.IsSelecionado =
                    false;
            }

            // =========================
            // NOVO
            // =========================

            _selecionado = vm;

            _selecionado.IsSelecionado =
                true;

            // =========================
            // EDITOR
            // =========================

            AppServices
                .Editor
                .ElementoSelecionado = vm;
        }

        // =========================
        // LIMPAR
        // =========================

        public static void Limpar()
        {
            if (_selecionado != null)
            {
                _selecionado.IsSelecionado =
                    false;
            }

            _selecionado = null;

            AppServices
                .Editor
                .ElementoSelecionado = null;
        }
    }
}