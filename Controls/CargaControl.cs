using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Controls.Base;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class CargaControl
        : ElementoControlBase
    {
        private readonly Rectangle _rectangle;

        private readonly DragService _drag;

        public Carga? Carga
        {
            get
            {
                if (DataContext is CargaViewModel vm)
                {
                    return (Carga)vm.Modelo;
                }

                return null;
            }
        }

        public CargaControl()
        {
            Width = 70;

            Height = 70;

            Cursor = Cursors.Hand;

            _rectangle = new Rectangle
            {
                Width = 70,

                Height = 70,

                RadiusX = 6,

                RadiusY = 6,

                Fill = CriarBrush("#E0A800"),

                Stroke = Brushes.White,

                StrokeThickness = 2
            };

            Content = _rectangle;

            _drag = new DragService(this);
        }

        protected override void
            AtualizarVisualSelecionado()
        {
            _rectangle.Stroke =
                Brushes.DeepSkyBlue;

            _rectangle.StrokeThickness =
                4;
        }

        protected override void
            AtualizarVisualNormal()
        {
            _rectangle.Stroke =
                Brushes.White;

            _rectangle.StrokeThickness =
                2;
        }
    }
}