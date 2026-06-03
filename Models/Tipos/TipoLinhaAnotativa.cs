namespace Araci.Models.Tipos
{
    public class TipoLinhaAnotativa : TipoElemento
    {
        public const string PARAM_ESTILO_LINHA = "EstiloLinha";

        public TipoLinhaAnotativa()
        {
            NomeTipo = "Linha contínua";
            Familia = "Anotações";
            Categoria = "Linhas";

            DefinirParametro(new Parameter<string>(PARAM_ESTILO_LINHA, "Contínuo"));
        }

        public string EstiloLinha
        {
            get => Obter<string>(PARAM_ESTILO_LINHA);
            set => Definir(PARAM_ESTILO_LINHA, NormalizarEstilo(value));
        }

        private static string NormalizarEstilo(string? valor)
        {
            return valor switch
            {
                "Tracejado" => "Tracejado",
                "Traço ponto" => "Traço ponto",
                "Traço dois pontos" => "Traço dois pontos",
                _ => "Contínuo"
            };
        }
    }
}
