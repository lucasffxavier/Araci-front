using System.Collections.Generic;

using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public static class TipoElementoViewModelFactory
    {
        private static readonly Dictionary<TipoElemento, TipoElementoViewModel>
            _cache = new();

        public static TipoElementoViewModel?
            Criar(TipoElemento? tipo)
        {
            if (tipo == null)
                return null;

            if (_cache.TryGetValue(
                    tipo,
                    out TipoElementoViewModel? existente))
            {
                return existente;
            }

            TipoElementoViewModel? criado =
                CriarNovo(tipo);

            if (criado != null)
            {
                _cache[tipo] =
                    criado;
            }

            return criado;
        }

        public static void Remover(
            TipoElemento tipo)
        {
            _cache.Remove(tipo);
        }

        public static void Limpar()
        {
            _cache.Clear();
        }

        private static TipoElementoViewModel?
            CriarNovo(TipoElemento tipo)
        {
            return tipo switch
            {
                TipoCabo cabo =>
                    new TipoCaboViewModel(cabo),

                TipoCarga carga =>
                    new TipoCargaViewModel(carga),

                TipoGerador gerador =>
                    new TipoGeradorViewModel(gerador),

                _ => null
            };
        }
    }
}
