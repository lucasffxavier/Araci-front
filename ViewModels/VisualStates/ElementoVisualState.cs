using System.Windows.Media;

namespace Araci.ViewModels.VisualStates
{
    public class ElementoVisualState
    {
        public bool IsSelecionado { get; private set; }
        public bool IsHover { get; private set; }

        public bool IsVisivel { get; set; } = true;
        public bool IsTravado { get; set; }

        public Brush StrokeBase { get; set; } = Brushes.DimGray;
        public double StrokeThicknessBase { get; set; } = 2;

        public Brush Stroke { get; private set; } = Brushes.DimGray;
        public double StrokeThickness { get; private set; } = 2;

        public ElementoVisualState()
        {
            AtualizarVisual();
        }

        public void AtualizarSelecao(bool selecionado)
        {
            IsSelecionado = selecionado;
            AtualizarVisual();
        }

        public void AtualizarHover(bool hover)
        {
            IsHover = hover;
            AtualizarVisual();
        }

        private void AtualizarVisual()
        {
            if (IsSelecionado)
            {
                Stroke = Brushes.DeepSkyBlue;
                StrokeThickness = 4;
                return;
            }

            if (IsHover)
            {
                Stroke = Brushes.LightSkyBlue;
                StrokeThickness = 3;
                return;
            }

            Stroke = StrokeBase;
            StrokeThickness = StrokeThicknessBase;
        }
    }
}