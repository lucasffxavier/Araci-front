namespace Araci.Models.Tipos
{
    public class TipoCabo : TipoElemento
    {
        public const string PARAM_FASES = "Fases";
        public const string PARAM_R1 = "R1";
        public const string PARAM_X1 = "X1";
        public const string PARAM_R0 = "R0";
        public const string PARAM_X0 = "X0";
        public const string PARAM_C1 = "C1";
        public const string PARAM_C0 = "C0";
        public const string PARAM_SECAO = "Secao";

        public TipoCabo()
        {
            NomeTipo = "LC-500MCM";
            Familia = "Cabos";
            Categoria = "Cabos";

            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
            DefinirParametro(new Parameter<double>(PARAM_R1, 0.12));
            DefinirParametro(new Parameter<double>(PARAM_X1, 0.09));
            DefinirParametro(new Parameter<double>(PARAM_R0, 0.36));
            DefinirParametro(new Parameter<double>(PARAM_X0, 0.27));
            DefinirParametro(new Parameter<double>(PARAM_C1, 0.001));
            DefinirParametro(new Parameter<double>(PARAM_C0, 0.0007));
            DefinirParametro(new Parameter<double>(PARAM_SECAO, 253));
        }

        public int Fases
        {
            get => Obter<int>(PARAM_FASES);
            set => Definir(PARAM_FASES, value <= 0 ? 1 : value);
        }

        public double R1
        {
            get => Obter<double>(PARAM_R1);
            set => Definir(PARAM_R1, value);
        }

        public double X1
        {
            get => Obter<double>(PARAM_X1);
            set => Definir(PARAM_X1, value);
        }

        public double R0
        {
            get => Obter<double>(PARAM_R0);
            set => Definir(PARAM_R0, value);
        }

        public double X0
        {
            get => Obter<double>(PARAM_X0);
            set => Definir(PARAM_X0, value);
        }

        public double C1
        {
            get => Obter<double>(PARAM_C1);
            set => Definir(PARAM_C1, value);
        }

        public double C0
        {
            get => Obter<double>(PARAM_C0);
            set => Definir(PARAM_C0, value);
        }

        public double Secao
        {
            get => Obter<double>(PARAM_SECAO);
            set => Definir(PARAM_SECAO, value);
        }
    }
}
