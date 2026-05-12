using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Controls.Base;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class GeradorControl
        : ElementoControlBase
    {
        private readonly Ellipse _ellipse;

        private readonly DragService _drag;

        public Gerador? Gerador
        {
            get
            {
                if (DataContext is GeradorViewModel vm)
                {
                    return (Gerador)vm.Modelo;
                }

                return null;
            }
        }

        public GeradorControl()
        {
            Width = 80;

            Height = 80;

            Cursor = Cursors.Hand;

            _ellipse = new Ellipse
            {
                Width = 80,

                Height = 80,

                Fill = CriarBrush("#007ACC"),

                Stroke = Brushes.White,

                StrokeThickness = 2
            };

            Content = _ellipse;

            _drag = new DragService(this);
        }

        protected override void
            AtualizarVisualSelecionado()
        {
            _ellipse.Stroke =
                Brushes.DeepSkyBlue;

            _ellipse.StrokeThickness =
                4;
        }

        protected override void
            AtualizarVisualNormal()
        {
            _ellipse.Stroke =
                Brushes.White;

            _ellipse.StrokeThickness =
                2;
        }
    }
}