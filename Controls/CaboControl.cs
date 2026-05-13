using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Controls.Base;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class CaboControl
        : ElementoControlBase
    {
        private readonly Line _line;

        private readonly Canvas _canvas;

        public CaboControl()
        {
            ClipToBounds = false;

            _line = new Line
            {
                Stroke = Brushes.Lime,
                StrokeThickness = 4
            };

            _canvas = new Canvas();

            _canvas.Children.Add(_line);

            Content = _canvas;

            Loaded += (_, __) =>
            {
                Atualizar();
            };

            DataContextChanged += (_, __) =>
            {
                Atualizar();
            };
        }

        protected override void OnRenderSizeChanged(
            System.Windows.SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Atualizar();
        }

        private void Atualizar()
        {
            if (DataContext is not CaboViewModel vm)
                return;

            double x1 = vm.X;
            double y1 = vm.Y;

            double x2 = vm.X2;
            double y2 = vm.Y2;

            double minX =
                Math.Min(x1, x2);

            double minY =
                Math.Min(y1, y2);

            double largura =
                Math.Abs(x2 - x1) + 8;

            double altura =
                Math.Abs(y2 - y1) + 8;

            Width = largura;
            Height = altura;

            _canvas.Width = largura;
            _canvas.Height = altura;

            _line.X1 = x1 - minX + 4;
            _line.Y1 = y1 - minY + 4;

            _line.X2 = x2 - minX + 4;
            _line.Y2 = y2 - minY + 4;
        }

        protected override void AtualizarVisualSelecionado()
        {
            _line.Stroke =
                Brushes.DeepSkyBlue;

            _line.StrokeThickness = 6;

            Atualizar();
        }

        protected override void AtualizarVisualNormal()
        {
            _line.Stroke =
                Brushes.Lime;

            _line.StrokeThickness = 4;

            Atualizar();
        }
    }
}