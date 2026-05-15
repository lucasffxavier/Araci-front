using System;

using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class ElementoFactory
    {
        // =========================
        // VIEWMODEL
        // =========================

        public static ElementoViewModel? CriarViewModel(Elemento modelo)
        {
            return modelo switch
            {
                Cabo cabo => new CaboViewModel(cabo),

                Carga carga => new CargaViewModel(carga),

                Gerador gerador => new GeradorViewModel(gerador),

                _ => null
            };
        }

        // =========================
        // CABO
        // =========================

        public static CaboViewModel CriarCaboVM()
        {
            return new CaboViewModel(new Cabo());
        }

        // =========================
        // CARGA
        // =========================

        public static CargaViewModel CriarCargaVM()
        {
            return new CargaViewModel(new Carga());
        }

        // =========================
        // GERADOR
        // =========================

        public static GeradorViewModel CriarGeradorVM()
        {
            return new GeradorViewModel(new Gerador());
        }
    }
}