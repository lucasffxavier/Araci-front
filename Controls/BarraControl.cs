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
    public class BarraControl : ElementoControlBase
    {
        private readonly Grid _root;
        private readonly SvgViewbox _svg;
        private readonly Border _overlay;
        private readonly Border _previewOverlay;
        private readonly Border _topHandle;
        private readonly Border _bottomHandle;

        public BarraControl()
        {
            Cursor = System.Windows.Input.Cursors.Hand;
            _svg = new SvgViewbox
            {
                Stretch = Stretch.Fill,
                Source = new Uri("pack://application:,,,/Assets/Svg/barra.svg")
            };
            _overlay = new Border
            {
                Background = Brushes.DeepSkyBlue,
                Opacity = 0.6,
                Visibility = Visibility.Collapsed
            };
            _previewOverlay = new Border
            {
                Background = Brushes.DeepSkyBlue,
                Opacity = 0.35,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false
            };
            _topHandle = CriarHandle(VerticalAlignment.Top);
            _bottomHandle = CriarHandle(VerticalAlignment.Bottom);
            _root = new Grid { ClipToBounds = false };
            _root.Children.Add(_svg);
            _root.Children.Add(_previewOverlay);
            _root.Children.Add(_overlay);
            _root.Children.Add(_topHandle);
            _root.Children.Add(_bottomHandle);
            Content = _root;
            Loaded += OnLoaded;
            ConfigurarBindings();
        }

        protected override bool UsaBindings => true;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _overlay.OpacityMask = new VisualBrush(_svg);
            _previewOverlay.OpacityMask = new VisualBrush(_svg);
        }

        private static Border CriarHandle(VerticalAlignment verticalAlignment)
        {
            return new Border
            {
                Width = 12,
                Height = 12,
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.White,
                Background = Brushes.DeepSkyBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = verticalAlignment,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false,
                Margin = verticalAlignment == VerticalAlignment.Top ? new Thickness(0, -6, 0, 0) : new Thickness(0, 0, 0, -6)
            };
        }

        private void ConfigurarBindings()
        {
            SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _svg.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _svg.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _svg.SetBinding(OpacityProperty, new Binding("IsPreview") { Converter = new BoolToOpacityConverter() });
            _previewOverlay.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _previewOverlay.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _previewOverlay.SetBinding(VisibilityProperty, new Binding("IsPreview") { Converter = new BooleanToVisibilityConverter() });
            var multi = new MultiBinding { Converter = new SelectionOrHoverToVisibilityConverter() };
            multi.Bindings.Add(new Binding("IsSelecionado"));
            multi.Bindings.Add(new Binding("IsHover"));
            _overlay.SetBinding(VisibilityProperty, multi);
            _topHandle.SetBinding(VisibilityProperty, new Binding("IsSelecionado") { Converter = new BooleanToVisibilityConverter() });
            _bottomHandle.SetBinding(VisibilityProperty, new Binding("IsSelecionado") { Converter = new BooleanToVisibilityConverter() });
        }
    }
}