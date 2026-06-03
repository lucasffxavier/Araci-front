namespace Araci.Models
{
    public class LinhaAnotativa : ElementoAnotativo
    {
        public const string PARAM_X2 = "X2";
        public const string PARAM_Y2 = "Y2";
        public const string PARAM_ESTILO_LINHA = "EstiloLinha";

        public LinhaAnotativa()
        {
            DefinirParametro(new Parameter<double>(PARAM_X2, 100.0));
            DefinirParametro(new Parameter<double>(PARAM_Y2, 0.0));
            DefinirParametro(new Parameter<string>(PARAM_ESTILO_LINHA, "Contínuo"));
        }

        public double X2
        {
            get => Obter<double>(PARAM_X2);
            set => Definir<double>(PARAM_X2, value);
        }

        public double Y2
        {
            get => Obter<double>(PARAM_Y2);
            set => Definir<double>(PARAM_Y2, value);
        }

        public string EstiloLinha
        {
            get => Obter<string>(PARAM_ESTILO_LINHA);
            set => Definir<string>(PARAM_ESTILO_LINHA, value);
        }

        public override Elemento Clonar()
        {
            var clone = new LinhaAnotativa();

            CopiarBasePara(clone);

            return clone;
        }
    }
}
