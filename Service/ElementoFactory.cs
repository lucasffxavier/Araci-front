using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class ElementoFactory
    {
        // =========================
        // CABO
        // =========================

        public static CaboViewModel CriarCaboVM()
        {
            return new CaboViewModel(
                new Cabo());
        }

        // =========================
        // CARGA
        // =========================

        public static CargaViewModel CriarCargaVM()
        {
            return new CargaViewModel(
                new Carga());
        }

        // =========================
        // GERADOR
        // =========================

        public static GeradorViewModel CriarGeradorVM()
        {
            return new GeradorViewModel(
                new Gerador());
        }
    }
}