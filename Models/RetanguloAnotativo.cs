using Araci.Models.Tipos;

namespace Araci.Models
{
    public class RetanguloAnotativo : ElementoAnotativoRetangular
    {
        public TipoLinhaAnotativa? TipoLinha => Tipo as TipoLinhaAnotativa;

        public override Elemento Clonar()
        {
            var clone = new RetanguloAnotativo();

            CopiarBasePara(clone);

            return clone;
        }
    }
}