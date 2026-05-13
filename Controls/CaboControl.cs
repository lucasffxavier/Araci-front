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
                Stroke = Brushes.Black,
                StrokeThickness = 4,
                SnapsToDevicePixels = true
            };

            _canvas = new Canvas();

            _canvas.Children.Add(_line);

            Content = _canvas;
        }

        private void Atualizar(
            CaboViewModel vm)
        {
            double minX =
                Math.Min(vm.X, vm.X2);

            double minY =
                Math.Min(vm.Y, vm.Y2);

            Width =
                Math.Max(8,
                    Math.Abs(vm.X2 - vm.X)) + 8;

            Height =
                Math.Max(8,
                    Math.Abs(vm.Y2 - vm.Y)) + 8;

            _canvas.Width = Width;
            _canvas.Height = Height;

            Canvas.SetLeft(this, minX);
            Canvas.SetTop(this, minY);

            _line.X1 = vm.X - minX + 4;
            _line.Y1 = vm.Y - minY + 4;

            _line.X2 = vm.X2 - minX + 4;
            _line.Y2 = vm.Y2 - minY + 4;
        }

        protected override void AplicarEstadoVisual(
            ElementoViewModel vm)
        {
            if (vm is not CaboViewModel cabo)
                return;

            _line.Stroke = vm.Stroke;

            _line.StrokeThickness =
                vm.StrokeThickness;

            Atualizar(cabo);
        }
    }
}