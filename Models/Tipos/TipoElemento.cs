namespace Araci.Models.Tipos
{
    public abstract class TipoElemento
    {
        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string NomeTipo { get; set; }
            = string.Empty;

        public string Familia { get; set; }
            = string.Empty;

        public string Categoria { get; set; }
            = string.Empty;
    }
}