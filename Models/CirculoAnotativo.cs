using Araci.Models.Tipos;

namespace Araci.Models
{
    public class CirculoAnotativo : ElementoAnotativo
    {
        public const string PARAM_RAIO = "Raio";

        public CirculoAnotativo()
        {
            DefinirParametro(new Parameter<double>(PARAM_RAIO, 50.0));
        }

        public double Raio
        {
            get => Obter<double>(PARAM_RAIO);
            set => Definir(PARAM_RAIO, value);
        }

        public double Diametro => Raio * 2.0;

        public TipoLinhaAnotativa? TipoLinha => Tipo as TipoLinhaAnotativa;

        public override Elemento Clonar()
        {
            var clone = new CirculoAnotativo();

            CopiarBasePara(clone);

            return clone;
        }
    }
}