using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;

using Araci.Controls.Base;

namespace Araci.Controls
{
    public class CargaControl : ElementoControlBase
    {
        private readonly Rectangle _rectangle;

        public CargaControl()
        {
            Cursor = Cursors.Hand;

            _rectangle = new Rectangle
            {
                RadiusX = 6,
                RadiusY = 6,
                Fill =
                    new SolidColorBrush(
                        (Color)ColorConverter
                            .ConvertFromString("#E0A800"))
            };

            Content = _rectangle;

            ConfigurarBindings();
        }

        protected override bool UsaBindings =>
            true;

        private void ConfigurarBindings()
        {
            SetBinding(
                WidthProperty,
                new Binding("RenderData.Largura"));

            SetBinding(
                HeightProperty,
                new Binding("RenderData.Altura"));

            _rectangle.SetBinding(
                WidthProperty,
                new Binding("RenderData.Largura"));

            _rectangle.SetBinding(
                HeightProperty,
                new Binding("RenderData.Altura"));

            _rectangle.SetBinding(
                Shape.StrokeProperty,
                new Binding("RenderData.Stroke"));

            _rectangle.SetBinding(
                Shape.StrokeThicknessProperty,
                new Binding("RenderData.StrokeThickness"));
        }
    }
}
