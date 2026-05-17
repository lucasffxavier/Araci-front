using System.Windows.Data;
using System.Windows.Input;

using Araci.Controls.Base;

using SharpVectors.Converters;

namespace Araci.Controls
{
    public class GeradorControl
        : ElementoControlBase
    {
        private readonly SvgViewbox _svg;

        public GeradorControl()
        {
            Cursor = Cursors.Hand;

            _svg = new SvgViewbox
            {
                Stretch = System.Windows.Media.Stretch.Fill,

                Source = new System.Uri(
                    "pack://application:,,,/Assets/Svg/gerador.svg",
                    System.UriKind.Absolute)
            };

            Content = _svg;

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

            _svg.SetBinding(
                WidthProperty,
                new Binding("RenderData.Largura"));

            _svg.SetBinding(
                HeightProperty,
                new Binding("RenderData.Altura"));
        }
    }
}