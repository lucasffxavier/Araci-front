using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class CableVertexInteractionController
    {
        private const double HandleTolerance = 8.0;

        private CaboViewModel? _caboAtivo;
        private int _indiceAtivo = -1;
        private Point _pontoInicialArrasto;
        private OrthogonalAxis? _eixoOrtogonal;

        public CaboViewModel? CaboAtivo => _caboAtivo;
        public int IndiceAtivo => _indiceAtivo;
        public bool IsDragging => _caboAtivo != null;

        public CableVertexHandleViewModel? HitTest(IEnumerable<CableVertexHandleViewModel> handles, Point position)
        {
            double tolerance2 = HandleTolerance * HandleTolerance;

            return handles
                .Where(h => DistanciaQuadrada(new Point(h.X, h.Y), position) <= tolerance2)
                .OrderBy(h => DistanciaQuadrada(new Point(h.X, h.Y), position))
                .FirstOrDefault();
        }

        public CableVertexSegmentHit? HitTestSegment(IEnumerable<CaboViewModel> cabos, Point position)
        {
            double tolerance2 = HandleTolerance * HandleTolerance;
            CableVertexSegmentHit? melhor = null;

            foreach (CaboViewModel cabo in cabos)
            {
                var vertices = cabo.Cabo.Vertices;

                if (cabo.IsPreview || vertices.Count < 2)
                    continue;

                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    Point projection = ProjetarNoSegmento(position, vertices[i], vertices[i + 1]);
                    double distance2 = DistanciaQuadrada(position, projection);

                    if (distance2 > tolerance2)
                        continue;

                    if (melhor == null || distance2 < melhor.DistanceSquared)
                        melhor = new CableVertexSegmentHit(cabo, i + 1, projection, distance2);
                }
            }

            return melhor;
        }

        public void BeginDrag(CableVertexHandleViewModel handle)
        {
            _caboAtivo = handle.Cabo;
            _indiceAtivo = handle.Indice;
            _pontoInicialArrasto = new Point(handle.X, handle.Y);
            _eixoOrtogonal = null;
        }

        public Point AplicarRestricaoOrtogonal(Point position, ToolInputState inputState)
        {
            if (!inputState.IsShiftPressed)
            {
                _eixoOrtogonal = null;
                return position;
            }

            Vector total = position - _pontoInicialArrasto;

            if (!_eixoOrtogonal.HasValue)
            {
                if (Math.Abs(total.X) < 0.0001 && Math.Abs(total.Y) < 0.0001)
                    return _pontoInicialArrasto;

                _eixoOrtogonal = Math.Abs(total.X) >= Math.Abs(total.Y)
                    ? OrthogonalAxis.Horizontal
                    : OrthogonalAxis.Vertical;
            }

            return _eixoOrtogonal == OrthogonalAxis.Horizontal
                ? new Point(position.X, _pontoInicialArrasto.Y)
                : new Point(_pontoInicialArrasto.X, position.Y);
        }

        public void ClearDrag()
        {
            _caboAtivo = null;
            _indiceAtivo = -1;
            _eixoOrtogonal = null;
        }

        public static bool IndiceIntermediarioValido(CaboViewModel cabo, int indice)
        {
            return indice > 0 && indice < cabo.Cabo.Vertices.Count - 1;
        }

        private static Point ProjetarNoSegmento(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;

            if (lengthSquared <= double.Epsilon)
                return a;

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));

            return new Point(
                a.X + t * dx,
                a.Y + t * dy);
        }

        private static double DistanciaQuadrada(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;

            return dx * dx + dy * dy;
        }

        private enum OrthogonalAxis
        {
            Horizontal,
            Vertical
        }
    }

    public sealed class CableVertexSegmentHit
    {
        public CableVertexSegmentHit(CaboViewModel cabo, int insertIndex, Point point, double distanceSquared)
        {
            Cabo = cabo;
            InsertIndex = insertIndex;
            Point = point;
            DistanceSquared = distanceSquared;
        }

        public CaboViewModel Cabo { get; }
        public int InsertIndex { get; }
        public Point Point { get; }
        public double DistanceSquared { get; }
    }
}
