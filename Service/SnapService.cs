// Service/SnapService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class SnapService
    {
        private readonly ISceneQueryService _queries;

        public SnapService(ISceneQueryService queries)
        {
            _queries = queries
                ?? throw new ArgumentNullException(nameof(queries));
        }

        public bool Habilitado { get; set; } = true;

        public double TerminalTolerance { get; set; } = 15;

        public Point Snap(Point point)
        {
            if (!Habilitado)
                return point;

            Point? terminal =
                SnapTerminal(point);

            return terminal ?? point;
        }

        public Point SnapFromElemento(
            ElementoViewModel? vm,
            Point fallback)
        {
            if (!Habilitado)
                return fallback;

            if (vm?.Modelo is ITerminalOwner owner)
            {
                Terminal? terminal =
                    ObterTerminalMaisProximo(
                        owner,
                        fallback);

                if (terminal != null)
                    return terminal.Posicao;
            }

            return Snap(fallback);
        }

        public Point SnapPoint(Point point)
        {
            return point;
        }

        public Vector SnapDelta(Vector delta)
        {
            return delta;
        }

        private Point? SnapTerminal(Point point)
        {
            Terminal? melhor = null;

            double menorDist = double.MaxValue;

            var elementos =
                _queries.Nearby(
                    point,
                    TerminalTolerance);

            foreach (Terminal terminal
                in EnumerarTerminais(elementos))
            {
                double dx =
                    terminal.Posicao.X - point.X;

                double dy =
                    terminal.Posicao.Y - point.Y;

                double dist =
                    dx * dx + dy * dy;

                if (dist >
                    TerminalTolerance * TerminalTolerance)
                {
                    continue;
                }

                if (dist >= menorDist)
                    continue;

                menorDist = dist;

                melhor = terminal;
            }

            return melhor?.Posicao;
        }

        private static IEnumerable<Terminal>
            EnumerarTerminais(
                IEnumerable<ElementoViewModel> elementos)
        {
            return elementos.SelectMany(
                e =>
                    (e.Modelo as ITerminalOwner)?.Terminais
                    ?? Enumerable.Empty<Terminal>());
        }

        private static Terminal?
            ObterTerminalMaisProximo(
                ITerminalOwner owner,
                Point point)
        {
            Terminal? melhor = null;

            double menorDist = double.MaxValue;

            foreach (Terminal terminal in owner.Terminais)
            {
                double dx =
                    terminal.Posicao.X - point.X;

                double dy =
                    terminal.Posicao.Y - point.Y;

                double dist =
                    dx * dx + dy * dy;

                if (dist >= menorDist)
                    continue;

                menorDist = dist;

                melhor = terminal;
            }

            return melhor;
        }
    }
}