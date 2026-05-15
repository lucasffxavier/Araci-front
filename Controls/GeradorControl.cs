using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;

using Araci.Controls.Base;

namespace Araci.Controls
{
    public class GeradorControl : ElementoControlBase
    {
        private readonly Ellipse _ellipse;

        public GeradorControl()
        {
            Cursor = Cursors.Hand;

            _ellipse = new Ellipse
            {
                Fill =
                    new SolidColorBrush(
                        (Color)ColorConverter
                            .ConvertFromString("#007ACC"))
            };

            Content = _ellipse;

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

            _ellipse.SetBinding(
                WidthProperty,
                new Binding("RenderData.Largura"));

            _ellipse.SetBinding(
                HeightProperty,
                new Binding("RenderData.Altura"));

            _ellipse.SetBinding(
                Shape.StrokeProperty,
                new Binding("RenderData.Stroke"));

            _ellipse.SetBinding(
                Shape.StrokeThicknessProperty,
                new Binding("RenderData.StrokeThickness"));
        }
    }
}
