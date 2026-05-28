namespace Araci.Models.Tipos
{
    public class TipoTransformador : TipoElemento
    {
        public const string PARAM_FASES = "Fases";
        public const string PARAM_ENROLAMENTOS = "Enrolamentos";
        public const string PARAM_R_PERCENTUAL = "RPercentual";
        public const string PARAM_X_PERCENTUAL = "XPercentual";
        public const string PARAM_LIGACAO_PRIMARIO = "LigacaoPrimario";
        public const string PARAM_LIGACAO_SECUNDARIO = "LigacaoSecundario";

        public TipoTransformador()
        {
            NomeTipo = "Transformador 2 Enrolamentos";
            Familia = "Transformadores";
            Categoria = "Transformadores";
            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
            DefinirParametro(new Parameter<int>(PARAM_ENROLAMENTOS, 2));
            DefinirParametro(new Parameter<double>(PARAM_R_PERCENTUAL, 1));
            DefinirParametro(new Parameter<double>(PARAM_X_PERCENTUAL, 5));
            DefinirParametro(new Parameter<string>(PARAM_LIGACAO_PRIMARIO, "Wye"));
            DefinirParametro(new Parameter<string>(PARAM_LIGACAO_SECUNDARIO, "Wye"));
        }

        public int Fases
        {
            get => Obter<int>(PARAM_FASES);
            set => Definir(PARAM_FASES, value <= 0 ? 3 : value);
        }

        public int Enrolamentos
        {
            get => Obter<int>(PARAM_ENROLAMENTOS);
            set => Definir(PARAM_ENROLAMENTOS, value <= 0 ? 2 : value);
        }

        public double RPercentual
        {
            get => Obter<double>(PARAM_R_PERCENTUAL);
            set => Definir(PARAM_R_PERCENTUAL, value < 0 ? 0 : value);
        }

        public double XPercentual
        {
            get => Obter<double>(PARAM_X_PERCENTUAL);
            set => Definir(PARAM_X_PERCENTUAL, value < 0 ? 0 : value);
        }

        public string LigacaoPrimario
        {
            get => Obter<string>(PARAM_LIGACAO_PRIMARIO);
            set => Definir(PARAM_LIGACAO_PRIMARIO, string.IsNullOrWhiteSpace(value) ? "Wye" : value);
        }

        public string LigacaoSecundario
        {
            get => Obter<string>(PARAM_LIGACAO_SECUNDARIO);
            set => Definir(PARAM_LIGACAO_SECUNDARIO, string.IsNullOrWhiteSpace(value) ? "Wye" : value);
        }
    }
}