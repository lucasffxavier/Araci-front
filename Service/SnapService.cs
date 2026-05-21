using System;
using System.Linq;
using System.Windows;
using Araci.Core.Scenes;
using Araci.Models;

namespace Araci.Services
{
    public class SnapService
    {
        public bool Habilitado { get; set; } = true;

        public double TerminalTolerance { get; set; } = 15;

        public Point Snap(Point point, Scene scene)
        {
            if (!Habilitado)
                return point;

            var terminal = SnapTerminal(point, scene);

            return terminal ?? point;
        }

        // 🔥 movimento agora é totalmente fluido
        public Point SnapPoint(Point point)
        {
            return point;
        }

        public Vector SnapDelta(Vector delta)
        {
            return delta;
        }

        private Point? SnapTerminal(Point point, Scene scene)
        {
            var terminais = scene.Elementos
                .SelectMany(e =>
                    (e.Modelo as ITerminalOwner)?.Terminais
                    ?? Enumerable.Empty<Terminal>());

            Terminal? melhor = null;

            double menorDist = double.MaxValue;

            foreach (var t in terminais)
            {
                double dx = t.Posicao.X - point.X;
                double dy = t.Posicao.Y - point.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < menorDist && dist <= TerminalTolerance)
                {
                    menorDist = dist;
                    melhor = t;
                }
            }

            return melhor?.Posicao;
        }
    }
}