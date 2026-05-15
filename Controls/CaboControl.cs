using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;

using Araci.Controls.Base;

namespace Araci.Controls
{
    public class CaboControl
        : ElementoControlBase
    {
        private readonly Canvas _canvas;

        private readonly Line _line;

        public CaboControl()
        {
            ClipToBounds = false;

            _canvas = new Canvas();

            _line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 4,
                SnapsToDevicePixels = true
            };

            _canvas.Children.Add(_line);

            Content = _canvas;

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

            _canvas.SetBinding(
                WidthProperty,
                new Binding("RenderData.Largura"));

            _canvas.SetBinding(
                HeightProperty,
                new Binding("RenderData.Altura"));

            _line.SetBinding(
                Line.X1Property,
                new Binding("RenderData.PontoLocalInicial.X"));

            _line.SetBinding(
                Line.Y1Property,
                new Binding("RenderData.PontoLocalInicial.Y"));

            _line.SetBinding(
                Line.X2Property,
                new Binding("RenderData.PontoLocalFinal.X"));

            _line.SetBinding(
                Line.Y2Property,
                new Binding("RenderData.PontoLocalFinal.Y"));

            _line.SetBinding(
                Shape.StrokeProperty,
                new Binding("RenderData.Stroke"));

            _line.SetBinding(
                Shape.StrokeThicknessProperty,
                new Binding("RenderData.StrokeThickness"));
        }
    }
}
