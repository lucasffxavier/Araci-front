using System;
using System.Windows.Data;
using System.Windows.Input;
using Araci.Controls.Base;
using SharpVectors.Converters;

namespace Araci.Controls
{
    public class CargaControl : ElementoControlBase
    {
        private readonly SvgViewbox _svg;

        public CargaControl()
        {
            Cursor = Cursors.Hand;

            _svg = new SvgViewbox
            {
                Stretch = System.Windows.Media.Stretch.Fill,
                Source = new Uri("pack://application:,,,/Araci;component/Assets/Svg/carga.svg", UriKind.Absolute)
            };

            Content = _svg;
            ConfigurarBindings();
        }

        protected override bool UsaBindings => true;

        private void ConfigurarBindings()
        {
            SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            SetBinding(HeightProperty, new Binding("RenderData.Altura"));

            _svg.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _svg.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
        }
    }
}