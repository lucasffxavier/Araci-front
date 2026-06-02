// Service/SnapService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.Services.Settings;
using Araci.ViewModels;

namespace Araci.Services
{
    public class SnapService
    {
        private readonly ISceneQueryService _queries;
        private readonly EditorSettings? _settings;
        private double _terminalTolerance = 15;

        public SnapService(ISceneQueryService queries)
            : this(queries, null)
        {
        }

        public SnapService(
            ISceneQueryService queries,
            EditorSettings? settings)
        {
            _queries = queries
                ?? throw new ArgumentNullException(nameof(queries));
            _settings = settings;
        }

        public bool Habilitado { get; set; } = true;

        public double TerminalTolerance
        {
            get => _settings?.ElectricalSnapTolerance ?? _terminalTolerance;
            set
            {
                if (_settings != null)
                    _settings.ElectricalSnapTolerance = value;
                else
                    _terminalTolerance = value;
            }
        }

        public Point Snap(Point point)
        {
            return Snap(point, null);
        }

        public Point Snap(Point point, ElementoViewModel? ignorar)
        {
            if (!Habilitado)
                return point;

            Terminal? terminal =
                SnapTerminal(point, ignorar);

            return terminal?.Posicao ?? point;
        }

        public Point SnapFromElemento(
            ElementoViewModel? vm,
            Point fallback)
        {
            return SnapFromElemento(vm, fallback, null);
        }

        public Point SnapFromElemento(
            ElementoViewModel? vm,
            Point fallback,
            ElementoViewModel? ignorar)
        {
            if (!Habilitado)
                return fallback;

            if (!ReferenceEquals(vm, ignorar) &&
                vm?.Modelo is ITerminalOwner owner)
            {
                Terminal? terminal =
                    ObterTerminalMaisProximo(
                        owner,
                        fallback);

                if (terminal != null)
                    return terminal.Posicao;
            }

            return Snap(fallback, ignorar);
        }

        public Terminal? ObterTerminalMaisProximo(
            ElementoViewModel? vm,
            Point point)
        {
            if (vm?.Modelo is not ITerminalOwner owner)
                return null;

            return ObterTerminalMaisProximo(owner, point);
        }

        public Terminal? ObterTerminalMaisProximo(
            Point point,
            ElementoViewModel? ignorar = null)
        {
            return ObterTerminalMaisProximo(point, ignorar, null);
        }

        public Terminal? ObterTerminalMaisProximo(
            Point point,
            ElementoViewModel? ignorar,
            Func<Terminal, bool>? filtro)
        {
            if (!Habilitado)
                return null;

            return SnapTerminal(point, ignorar, filtro);
        }

        public Point SnapPoint(Point point)
        {
            return point;
        }

        public Vector SnapDelta(Vector delta)
        {
            return delta;
        }

        private Terminal? SnapTerminal(Point point, ElementoViewModel? ignorar)
        {
            return SnapTerminal(point, ignorar, null);
        }

        private Terminal? SnapTerminal(
            Point point,
            ElementoViewModel? ignorar,
            Func<Terminal, bool>? filtro)
        {
            Terminal? melhor = null;

            double menorDist = double.MaxValue;

            var elementos =
                _queries.Nearby(
                    point,
                    TerminalTolerance);

            foreach (Terminal terminal
                in EnumerarTerminais(elementos, ignorar))
            {
                if (filtro != null && !filtro(terminal))
                    continue;

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

                if (melhor != null &&
                    !EhMelhorTerminal(terminal, dist, melhor, menorDist))
                {
                    continue;
                }

                menorDist = dist;

                melhor = terminal;
            }

            return melhor;
        }

        private static bool EhMelhorTerminal(
            Terminal candidato,
            double distancia,
            Terminal atual,
            double distanciaAtual)
        {
            if (distancia < distanciaAtual - 0.000001)
                return true;

            if (Math.Abs(distancia - distanciaAtual) > 0.000001)
                return false;

            return PrioridadeTerminal(candidato) < PrioridadeTerminal(atual);
        }

        private static int PrioridadeTerminal(Terminal terminal)
        {
            return terminal.Kind == TerminalKind.Electrical ? 0 : 1;
        }

        private static IEnumerable<Terminal>
            EnumerarTerminais(
                IEnumerable<ElementoViewModel> elementos,
                ElementoViewModel? ignorar)
        {
            return elementos
                .Where(e => !ReferenceEquals(e, ignorar))
                .SelectMany(
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
