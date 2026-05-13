using System.Windows.Media;

namespace Araci.ViewModels.VisualStates
{
    public class ElementoVisualState
    {
        // =========================
        // FLAGS
        // =========================

        public bool IsSelecionado
        { get; private set; }

        public bool IsHover
        { get; set; }

        public bool IsVisivel
        { get; set; }
            = true;

        public bool IsTravado
        { get; set; }

        // =========================
        // VISUAL BASE
        // =========================

        public Brush StrokeBase
        { get; set; }
            = Brushes.DimGray;

        public double StrokeThicknessBase
        { get; set; }
            = 2;

        // =========================
        // VISUAL ATUAL
        // =========================

        public Brush Stroke
        { get; private set; }
            = Brushes.DimGray;

        public double StrokeThickness
        { get; private set; }
            = 2;

        // =========================
        // CONSTRUTOR
        // =========================

        public ElementoVisualState()
        {
            RestaurarVisualBase();
        }

        // =========================
        // VISUAL BASE
        // =========================

        public void DefinirVisualBase(
            Brush stroke,
            double thickness)
        {
            StrokeBase = stroke;

            StrokeThicknessBase = thickness;

            if (!IsSelecionado)
            {
                RestaurarVisualBase();
            }
        }

        // =========================
        // SELEÇÃO
        // =========================

        public void AtualizarSelecao(
            bool selecionado)
        {
            IsSelecionado = selecionado;

            if (selecionado)
            {
                Stroke =
                    Brushes.DeepSkyBlue;

                StrokeThickness = 4;
            }
            else
            {
                RestaurarVisualBase();
            }
        }

        // =========================
        // RESTAURAR
        // =========================

        private void RestaurarVisualBase()
        {
            Stroke = StrokeBase;

            StrokeThickness =
                StrokeThicknessBase;
        }
    }
}