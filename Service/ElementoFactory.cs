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

        public static ElementoViewModel? CriarViewModel(
            Elemento modelo)
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

        public static Cabo CriarCabo()
        {
            var cabo =
                new Cabo
                {
                    Tipo =
                        AppServices.Types.TipoCaboPadrao
                        ?? throw new InvalidOperationException(
                            "Nenhum tipo de cabo cadastrado.")
                };

            return cabo;
        }

        public static CaboViewModel CriarCaboVM()
        {
            return new CaboViewModel(
                CriarCabo());
        }

        // =========================
        // CARGA
        // =========================

        public static Carga CriarCarga()
        {
            var carga =
                new Carga
                {
                    Tipo =
                        AppServices.Types.TipoCargaPadrao
                        ?? throw new InvalidOperationException(
                            "Nenhum tipo de carga cadastrado.")
                };

            return carga;
        }

        public static CargaViewModel CriarCargaVM()
        {
            return new CargaViewModel(
                CriarCarga());
        }

        // =========================
        // GERADOR
        // =========================

        public static Gerador CriarGerador()
        {
            var gerador =
                new Gerador
                {
                    Tipo =
                        AppServices.Types.TipoGeradorPadrao
                        ?? throw new InvalidOperationException(
                            "Nenhum tipo de gerador cadastrado.")
                };

            return gerador;
        }

        public static GeradorViewModel CriarGeradorVM()
        {
            return new GeradorViewModel(
                CriarGerador());
        }
    }
}
