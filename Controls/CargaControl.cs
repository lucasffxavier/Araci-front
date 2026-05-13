using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Controls.Base;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class CargaControl : ElementoControlBase
    {
        private readonly Rectangle _rectangle;

        public CargaControl()
        {
            Width = 70;
            Height = 70;

            Cursor = Cursors.Hand;

            _rectangle = new Rectangle
            {
                Width = 70,
                Height = 70,
                RadiusX = 6,
                RadiusY = 6,
                Fill =
                    new SolidColorBrush(
                        (Color)ColorConverter
                            .ConvertFromString("#E0A800"))
            };

            Content = _rectangle;
        }

        protected override void AplicarEstadoVisual(
            ElementoViewModel vm)
        {
            _rectangle.Stroke = vm.Stroke;
            _rectangle.StrokeThickness = vm.StrokeThickness;
        }
    }
}