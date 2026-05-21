using Araci.Controls.Base;
using Araci.Controls.Converters;
using SharpVectors.Converters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Araci.Controls
{
    public class GeradorControl : ElementoControlBase
    {
        private readonly Grid _root;
        private readonly SvgViewbox _svg;
        private readonly Border _overlay;

        public GeradorControl()
        {
            Cursor = Cursors.Hand;

            _svg = new SvgViewbox
            {
                Stretch = Stretch.Fill,
                Source = new Uri("pack://application:,,,/Assets/Svg/gerador.svg")
            };

            _overlay = new Border
            {
                Background = Brushes.DeepSkyBlue,
                Opacity = 0.6,
                Visibility = Visibility.Collapsed
            };

            _root = new Grid();
            _root.Children.Add(_svg);
            _root.Children.Add(_overlay);

            Content = _root;

            Loaded += OnLoaded;

            ConfigurarBindings();
        }

        protected override bool UsaBindings => true;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _overlay.OpacityMask = new VisualBrush(_svg);
        }

        private void ConfigurarBindings()
        {
            SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            SetBinding(HeightProperty, new Binding("RenderData.Altura"));

            _svg.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _svg.SetBinding(HeightProperty, new Binding("RenderData.Altura"));

            var multi = new MultiBinding
            {
                Converter = new SelectionOrHoverToVisibilityConverter()
            };

            multi.Bindings.Add(new Binding("IsSelecionado"));
            multi.Bindings.Add(new Binding("IsHover"));

            _overlay.SetBinding(VisibilityProperty, multi);
        }
    }
}