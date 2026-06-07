using Araci.Controls.Base;
using Araci.Controls.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Araci.Controls
{
    public class BarraControl : ElementoControlBase
    {
        private const double HandleSize = 12;
        private readonly Grid _root;
        private readonly Border _body;
        private readonly Border _overlay;
        private readonly Border _previewOverlay;
        private readonly Border _topHandle;
        private readonly Border _bottomHandle;

        public BarraControl()
        {
            Cursor = System.Windows.Input.Cursors.Hand;
            _body = new Border
            {
                Background = Brushes.Black,
                IsHitTestVisible = false
            };
            _overlay = new Border
            {
                Background = Brushes.DeepSkyBlue,
                Opacity = 0.6,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false
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
            _root.Children.Add(_body);
            _root.Children.Add(_previewOverlay);
            _root.Children.Add(_overlay);
            _root.Children.Add(_topHandle);
            _root.Children.Add(_bottomHandle);
            Content = _root;
            ConfigurarBindings();
        }

        protected override bool UsaBindings => true;

        private static Border CriarHandle(VerticalAlignment verticalAlignment)
        {
            double handleOffset = -HandleSize / 2;

            return new Border
            {
                Width = HandleSize,
                Height = HandleSize,
                CornerRadius = new CornerRadius(HandleSize / 2),
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.White,
                Background = Brushes.DeepSkyBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = verticalAlignment,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false,
                Margin = verticalAlignment == VerticalAlignment.Top ? new Thickness(0, handleOffset, 0, 0) : new Thickness(0, 0, 0, handleOffset)
            };
        }

        private void ConfigurarBindings()
        {
            SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _body.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _body.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _body.SetBinding(OpacityProperty, new Binding("IsPreview") { Converter = new BoolToOpacityConverter() });
            _previewOverlay.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _previewOverlay.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            _previewOverlay.SetBinding(VisibilityProperty, new Binding("IsPreview") { Converter = new BooleanToVisibilityConverter() });
            _overlay.SetBinding(WidthProperty, new Binding("RenderData.Largura"));
            _overlay.SetBinding(HeightProperty, new Binding("RenderData.Altura"));
            var multi = new MultiBinding { Converter = new SelectionOrHoverToVisibilityConverter() };
            multi.Bindings.Add(new Binding("IsSelecionado"));
            multi.Bindings.Add(new Binding("IsHover"));
            _overlay.SetBinding(VisibilityProperty, multi);
            _topHandle.SetBinding(VisibilityProperty, new Binding("IsSelecionado") { Converter = new BooleanToVisibilityConverter() });
            _bottomHandle.SetBinding(VisibilityProperty, new Binding("IsSelecionado") { Converter = new BooleanToVisibilityConverter() });
        }
    }
}
