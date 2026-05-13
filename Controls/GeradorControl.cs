using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Controls.Base;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class GeradorControl : ElementoControlBase
    {
        private readonly Ellipse _ellipse;

        public GeradorControl()
        {
            Width = 80;
            Height = 80;

            Cursor = Cursors.Hand;

            _ellipse = new Ellipse
            {
                Width = 80,
                Height = 80,
                Fill =
                    new SolidColorBrush(
                        (Color)ColorConverter
                            .ConvertFromString("#007ACC"))
            };

            Content = _ellipse;
        }

        protected override void AplicarEstadoVisual(
            ElementoViewModel vm)
        {
            _ellipse.Stroke = vm.Stroke;
            _ellipse.StrokeThickness = vm.StrokeThickness;
        }
    }
}