using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class CaboNode : ElementoNode
    {
        private readonly Cabo _cabo;
        private double _x;
        private double _y;

        public CaboNode(Cabo cabo)
            : base(cabo)
        {
            _cabo = cabo ?? throw new ArgumentNullException(nameof(cabo));

            InicializarPosicao();
            AtualizarGeometria();
        }

        public override double X => _x;
        public override double Y => _y;
        public override double Largura => Bounds.Width;
        public override double Altura => Bounds.Height;

        private void InicializarPosicao()
        {
            if (_cabo.Vertices.Count == 0)
            {
                _x = 0;
                _y = 0;
                return;
            }

            var p = _cabo.Vertices[0];

            _x = p.X;
            _y = p.Y;
        }

        public override void Mover(Vector delta)
        {
            if (delta.X == 0 && delta.Y == 0)
                return;

            _x += delta.X;
            _y += delta.Y;

            for (int i = 0; i < _cabo.Vertices.Count; i++)
            {
                var p = _cabo.Vertices[i];

                _cabo.Vertices[i] = new Point(
                    p.X + delta.X,
                    p.Y + delta.Y);
            }

            if (_cabo.PreviewPonto.HasValue)
            {
                var preview = _cabo.PreviewPonto.Value;

                _cabo.PreviewPonto = new Point(
                    preview.X + delta.X,
                    preview.Y + delta.Y);
            }

            AtualizarGeometria();
        }

        public override void AtualizarGeometria()
        {
            var pontos = new List<Point>();

            pontos.AddRange(_cabo.Vertices);

            if (_cabo.PreviewPonto.HasValue)
                pontos.Add(_cabo.PreviewPonto.Value);

            if (pontos.Count == 0)
            {
                Bounds = Rect.Empty;
                return;
            }

            double minX = pontos.Min(p => p.X);
            double minY = pontos.Min(p => p.Y);
            double maxX = pontos.Max(p => p.X);
            double maxY = pontos.Max(p => p.Y);

            double largura = Math.Max(1, maxX - minX);
            double altura = Math.Max(1, maxY - minY);

            Bounds = new Rect(
                minX,
                minY,
                largura,
                altura);
        }
    }
}