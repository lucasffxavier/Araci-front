using System;
using System.Windows;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class TextoAnotativoNode : ElementoNode
    {
        private readonly TextoAnotativo _texto;

        public TextoAnotativoNode(TextoAnotativo texto)
            : base(texto)
        {
            _texto = texto ?? throw new ArgumentNullException(nameof(texto));
            AtualizarGeometria();
        }

        public override Rect BoundsAlinhamento
        {
            get
            {
                double y = Bounds.Top + Bounds.Height / 2;
                return new Rect(Bounds.Left, y, Bounds.Width, 1);
            }
        }

        public override double X
        {
            get => _texto.PosicaoX;
            set
            {
                _texto.PosicaoX = value;
                AtualizarGeometria();
            }
        }

        public override double Y
        {
            get => _texto.PosicaoY;
            set
            {
                _texto.PosicaoY = value;
                AtualizarGeometria();
            }
        }

        public override void AtualizarGeometria()
        {
            Bounds = new Rect(_texto.PosicaoX, _texto.PosicaoY, Math.Max(1, _texto.LarguraEstimada), Math.Max(1, _texto.AlturaEstimada));
        }
    }
}
