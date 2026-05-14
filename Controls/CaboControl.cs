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
        }

        protected override void AplicarEstadoVisual(
            ElementoViewModel vm)
        {
            Width =
                vm.Largura;

            Height =
                vm.Altura;

            _canvas.Width =
                vm.Largura;

            _canvas.Height =
                vm.Altura;

            _line.X1 =
                vm.Geometry.PontoLocalInicial.X;

            _line.Y1 =
                vm.Geometry.PontoLocalInicial.Y;

            _line.X2 =
                vm.Geometry.PontoLocalFinal.X;

            _line.Y2 =
                vm.Geometry.PontoLocalFinal.Y;

            _line.Stroke =
                vm.Stroke;

            _line.StrokeThickness =
                vm.StrokeThickness;
        }
    }
}