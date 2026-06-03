using System;
using System.Windows;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class CirculoAnotativoNode : ElementoNode
    {
        private readonly CirculoAnotativo _circulo;

        public CirculoAnotativoNode(CirculoAnotativo circulo)
            : base(circulo)
        {
            _circulo = circulo ?? throw new ArgumentNullException(nameof(circulo));
            AtualizarGeometria();
        }

        public override double X
        {
            get => _circulo.PosicaoX;
            set
            {
                _circulo.PosicaoX = value;
                AtualizarGeometria();
            }
        }

        public override double Y
        {
            get => _circulo.PosicaoY;
            set
            {
                _circulo.PosicaoY = value;
                AtualizarGeometria();
            }
        }

        public override void AtualizarGeometria()
        {
            double raio = Math.Max(1, Math.Abs(_circulo.Raio));
            Bounds = new Rect(_circulo.PosicaoX - raio, _circulo.PosicaoY - raio, raio * 2, raio * 2);
        }
    }
}