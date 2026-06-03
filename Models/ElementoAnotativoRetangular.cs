namespace Araci.Models
{
    public abstract class ElementoAnotativoRetangular : ElementoAnotativo
    {
        public const string PARAM_LARGURA = "Largura";
        public const string PARAM_ALTURA = "Altura";

        protected ElementoAnotativoRetangular()
        {
            DefinirParametro(new Parameter<double>(PARAM_LARGURA, 100.0));
            DefinirParametro(new Parameter<double>(PARAM_ALTURA, 50.0));
        }

        public double Largura
        {
            get => Obter<double>(PARAM_LARGURA);
            set => Definir(PARAM_LARGURA, value);
        }

        public double Altura
        {
            get => Obter<double>(PARAM_ALTURA);
            set => Definir(PARAM_ALTURA, value);
        }
    }
}
