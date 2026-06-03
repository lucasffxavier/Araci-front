namespace Araci.Models
{
    public abstract class ElementoAnotativo : Elemento
    {
        public const string PARAM_COR_LINHA = "CorLinha";
        public const string PARAM_ESPESSURA_LINHA = "EspessuraLinha";
        public const string PARAM_VISIVEL = "Visivel";

        protected ElementoAnotativo()
        {
            DefinirParametro(new Parameter<string>(PARAM_COR_LINHA, "#FF000000"));
            DefinirParametro(new Parameter<double>(PARAM_ESPESSURA_LINHA, 1.0));
            DefinirParametro(new Parameter<bool>(PARAM_VISIVEL, true));
        }

        public override ElementoDomainRole DomainRole => ElementoDomainRole.Anotacao;

        public string CorLinha
        {
            get => Obter<string>(PARAM_COR_LINHA);
            set => Definir(PARAM_COR_LINHA, value);
        }

        public double EspessuraLinha
        {
            get => Obter<double>(PARAM_ESPESSURA_LINHA);
            set => Definir(PARAM_ESPESSURA_LINHA, value);
        }

        public bool Visivel
        {
            get => Obter<bool>(PARAM_VISIVEL);
            set => Definir(PARAM_VISIVEL, value);
        }
    }
}
