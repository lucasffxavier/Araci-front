using System;
using System.Linq;
using System.Windows;
using Araci.Core.Scenes;
using Araci.Models;
using Araci.ViewModels;

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

        public Point SnapFromElemento(
            ElementoViewModel? vm,
            Point fallback,
            Scene scene)
        {
            if (!Habilitado)
                return fallback;

            // 🎯 PRIORIDADE TOTAL: elemento clicado
            if (vm?.Modelo is ITerminalOwner owner &&
                owner.Terminais.Count > 0)
            {
                return owner.Terminais[0].Posicao;
            }

            // fallback normal
            return Snap(fallback, scene);
        }

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
                double dist = dx * dx + dy * dy;

                if (dist < menorDist &&
                    dist <= TerminalTolerance * TerminalTolerance)
                {
                    menorDist = dist;
                    melhor = t;
                }
            }

            return melhor?.Posicao;
        }
    }
}