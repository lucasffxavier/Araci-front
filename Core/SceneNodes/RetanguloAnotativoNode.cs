using System;
using System.Windows;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class RetanguloAnotativoNode : ElementoNode
    {
        private readonly RetanguloAnotativo _retangulo;

        public RetanguloAnotativoNode(RetanguloAnotativo retangulo)
            : base(retangulo)
        {
            _retangulo = retangulo ?? throw new ArgumentNullException(nameof(retangulo));
            AtualizarGeometria();
        }

        public override double X
        {
            get => _retangulo.PosicaoX;
            set
            {
                _retangulo.PosicaoX = value;
                AtualizarGeometria();
            }
        }

        public override double Y
        {
            get => _retangulo.PosicaoY;
            set
            {
                _retangulo.PosicaoY = value;
                AtualizarGeometria();
            }
        }

        public override void AtualizarGeometria()
        {
            double largura = Math.Max(1, Math.Abs(_retangulo.Largura));
            double altura = Math.Max(1, Math.Abs(_retangulo.Altura));
            double x = _retangulo.Largura >= 0 ? _retangulo.PosicaoX : _retangulo.PosicaoX + _retangulo.Largura;
            double y = _retangulo.Altura >= 0 ? _retangulo.PosicaoY : _retangulo.PosicaoY + _retangulo.Altura;

            Bounds = new Rect(x, y, largura, altura);
        }
    }
}