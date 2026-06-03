using System;
using System.Windows;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class LinhaAnotativaNode : ElementoNode
    {
        private readonly LinhaAnotativa _linha;
        private double _x;
        private double _y;

        public LinhaAnotativaNode(LinhaAnotativa linha)
            : base(linha)
        {
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
            AtualizarGeometria();
        }

        public override double X
        {
            get => _x;
            set
            {
                double delta = value - _x;
                _linha.PosicaoX += delta;
                AtualizarGeometria();
            }
        }

        public override double Y
        {
            get => _y;
            set
            {
                double delta = value - _y;
                _linha.PosicaoY += delta;
                AtualizarGeometria();
            }
        }

        public Point PontoInicial => new(_linha.PosicaoX, _linha.PosicaoY);

        public Point PontoFinal => new(_linha.PosicaoX + _linha.X2, _linha.PosicaoY + _linha.Y2);

        public override void AtualizarGeometria()
        {
            Point inicio = PontoInicial;
            Point fim = PontoFinal;

            double minX = Math.Min(inicio.X, fim.X);
            double minY = Math.Min(inicio.Y, fim.Y);
            double maxX = Math.Max(inicio.X, fim.X);
            double maxY = Math.Max(inicio.Y, fim.Y);
            double largura = Math.Max(1, maxX - minX);
            double altura = Math.Max(1, maxY - minY);

            _x = minX;
            _y = minY;
            Bounds = new Rect(minX, minY, largura, altura);
        }
    }
}
