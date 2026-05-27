using Araci.Controls.Base;
using Araci.Controls.Converters;
using SharpVectors.Converters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Araci.Controls
{
    public class TransformadorControl : ElementoControlBase
    {
        private static readonly Uri SvgSource =
            new("pack://application:,,,/Assets/Svg/transformador.svg");

        private readonly Grid _root;
        private readonly SvgViewbox _svg;
        private readonly Border _overlay;
        private readonly Border _previewOverlay;

        public TransformadorControl()
        {
            Cursor = System.Windows.Input.Cursors.Hand;

            _svg = new SvgViewbox
            {
                Stretch = Stretch.Uniform,
                Source = SvgSource
            };

            _previewOverlay = new Border
            {
                Background = Brushes.DeepSkyBlue,
                Opacity = 0.35,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _overlay = new Border
            {
                Background = Brushes.DeepSkyBlue,
                Opacity = 0.6,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _root = new Grid();
            _root.Children.Add(_svg);
            _root.Children.Add(_previewOverlay);
            _root.Children.Add(_overlay);
            Content = _root;

            ConfigurarBindings();
            ConfigurarMascaras();
        }

        protected override bool UsaBindings => true;

        private void ConfigurarBindings()
        {
            SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            SetBinding(HeightProperty, new Binding("RenderData.Altura"));

            _svg.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _svg.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _svg.SetBinding(OpacityProperty, new Binding("IsPreview")
            {
                Converter = new BoolToOpacityConverter()
            });

            _previewOverlay.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _previewOverlay.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _previewOverlay.SetBinding(VisibilityProperty, new Binding("IsPreview")
            {
                Converter = new BooleanToVisibilityConverter()
            });

            _overlay.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _overlay.SetBinding(HeightProperty, new Binding("RenderData.Altura"));

            var multi = new MultiBinding
            {
                Converter = new SelectionOrHoverToVisibilityConverter()
            };

            multi.Bindings.Add(new Binding("IsSelecionado"));
            multi.Bindings.Add(new Binding("IsHover"));

            _overlay.SetBinding(VisibilityProperty, multi);
        }

        private void ConfigurarMascaras()
        {
            _previewOverlay.OpacityMask = CriarMascara();
            _overlay.OpacityMask = CriarMascara();
        }

        private VisualBrush CriarMascara()
        {
            return new VisualBrush(_svg)
            {
                Stretch = Stretch.Uniform,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };
        }
    }
}
