using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public static class TipoElementoViewModelFactory
    {
        public static TipoElementoViewModel?
            Criar(TipoElemento? tipo)
        {
            if (tipo == null)
                return null;

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