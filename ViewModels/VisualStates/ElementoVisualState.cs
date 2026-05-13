namespace Araci.ViewModels.VisualStates
{
    public class ElementoVisualState
    {
        // =========================
        // SELEÇÃO
        // =========================

        public bool IsSelecionado
        { get; set; }

        // =========================
        // FUTURO
        // =========================

        public bool IsHover
        { get; set; }

        public bool IsVisivel
        { get; set; }
            = true;

        public bool IsTravado
        { get; set; }
    }
}