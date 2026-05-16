using System;
using System.Windows;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class CaboNode : ElementoNode
    {
        private readonly Cabo _cabo;

        public CaboNode(Cabo cabo)
            : base(cabo)
        {
            _cabo = cabo;

            AtualizarGeometria();
        }

        public Point PontoInicial =>
            new(_cabo.PosicaoX, _cabo.PosicaoY);

        public Point PontoFinal =>
            new(_cabo.PosicaoX2, _cabo.PosicaoY2);

        public override double Largura =>
            Math.Abs(_cabo.PosicaoX2 - _cabo.PosicaoX);

        public override double Altura =>
            Math.Abs(_cabo.PosicaoY2 - _cabo.PosicaoY);

        public override void Mover(Vector delta)
        {
            _cabo.PosicaoX += delta.X;
            _cabo.PosicaoY += delta.Y;
            _cabo.PosicaoX2 += delta.X;
            _cabo.PosicaoY2 += delta.Y;

            AtualizarGeometria();
        }

        public override void AtualizarGeometria()
        {
            double x = Math.Min(_cabo.PosicaoX, _cabo.PosicaoX2);
            double y = Math.Min(_cabo.PosicaoY, _cabo.PosicaoY2);

            double largura =
                Math.Abs(_cabo.PosicaoX2 - _cabo.PosicaoX);

            double altura =
                Math.Abs(_cabo.PosicaoY2 - _cabo.PosicaoY);

            Bounds = new Rect(x, y, largura, altura);
        }
    }
}