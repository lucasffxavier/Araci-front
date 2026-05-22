using System;

using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ElementoFactory
    {
        private readonly TypeLibraryService _types;

        public ElementoFactory(TypeLibraryService types)
        {
            _types = types
                ?? throw new ArgumentNullException(nameof(types));
        }

        // =========================
        // VIEWMODEL
        // =========================

        public ElementoViewModel? CriarViewModel(
            Elemento modelo)
        {
            return modelo switch
            {
                Cabo cabo => new CaboViewModel(
                    cabo,
                    _types),

                Carga carga => new CargaViewModel(
                    carga,
                    _types),

                Gerador gerador => new GeradorViewModel(
                    gerador,
                    _types),

                Barra barra => new BarraViewModel(barra, _types),

                _ => null
            };
        }

        // =========================
        // CABO
        // =========================

        public Cabo CriarCabo()
        {
            return new Cabo
            {
                Tipo = _types.TipoCaboPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de cabo cadastrado.")
            };
        }

        public CaboViewModel CriarCaboVM()
        {
            return new CaboViewModel(
                CriarCabo(),
                _types);
        }

        // =========================
        // CARGA
        // =========================

        public Carga CriarCarga()
        {
            return new Carga
            {
                Tipo =
                    _types.TipoCargaPadrao
                    ?? throw new InvalidOperationException(
                        "Nenhum tipo de carga cadastrado.")
            };
        }

        public CargaViewModel CriarCargaVM()
        {
            return new CargaViewModel(
                CriarCarga(),
                _types);
        }

        // =========================
        // GERADOR
        // =========================

        public Gerador CriarGerador()
        {
            return new Gerador
            {
                Tipo =
                    _types.TipoGeradorPadrao
                    ?? throw new InvalidOperationException(
                        "Nenhum tipo de gerador cadastrado.")
            };
        }

        public GeradorViewModel CriarGeradorVM()
        {
            return new GeradorViewModel(
                CriarGerador(),
                _types);
        }

        public Barra CriarBarra()
        {
            return new Barra
            {
                Tipo = _types.TipoBarraPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de barra cadastrado.")
            };
        }

        public BarraViewModel CriarBarraVM()
        {
            return new BarraViewModel(
                CriarBarra(),
                _types);
        }
    }
}
