using System;
using System.Windows;
using Araci.Core.SceneQueries;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelectionBoxController
    {
        private readonly SelectionBoxViewModel _selectionBox;
        private readonly ISceneQueryService _queries;
        private readonly SelectionService _selection;

        private Point _inicio;
        private bool _adicionarAoExistente;

        public SelectionBoxController(
            SelectionBoxViewModel selectionBox,
            ISceneQueryService queries,
            SelectionService selection)
        {
            _selectionBox = selectionBox ?? throw new ArgumentNullException(nameof(selectionBox));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public bool IsActive { get; private set; }

        public void Begin(Point position, bool adicionarAoExistente)
        {
            _adicionarAoExistente = adicionarAoExistente;

            if (!_adicionarAoExistente)
                _selection.Limpar();

            _inicio = position;
            IsActive = true;

            _selectionBox.Visivel = true;
            _selectionBox.Atualizar(position, position);
        }

        public void Update(Point position)
        {
            if (!IsActive)
                return;

            _selectionBox.Atualizar(_inicio, position);
        }

        public void End()
        {
            if (!IsActive)
                return;

            Rect bounds = _selectionBox.Bounds;

            foreach (ElementoViewModel item in _queries.Query(bounds))
            {
                if (IntersectaSelecao(item, bounds))
                    _selection.Selecionar(item, true);
            }

            Cancel();
        }

        public void Cancel()
        {
            IsActive = false;
            _selectionBox.Visivel = false;
        }

        private static bool IntersectaSelecao(ElementoViewModel vm, Rect area)
        {
            if (vm is CaboViewModel cabo)
                return IntersectaCabo(cabo, area);

            return vm.Bounds.IntersectsWith(area);
        }

        private static bool IntersectaCabo(CaboViewModel cabo, Rect area)
        {
            var vertices = cabo.Cabo.Vertices;

            if (vertices.Count < 2)
                return cabo.Bounds.IntersectsWith(area);

            for (int i = 0; i < vertices.Count; i++)
            {
                if (area.Contains(vertices[i]))
                    return true;
            }

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                if (SegmentoIntersectaRect(vertices[i], vertices[i + 1], area))
                    return true;
            }

            return false;
        }

        private static bool SegmentoIntersectaRect(Point a, Point b, Rect rect)
        {
            if (rect.Contains(a) || rect.Contains(b))
                return true;

            Point topLeft = new(rect.Left, rect.Top);
            Point topRight = new(rect.Right, rect.Top);
            Point bottomRight = new(rect.Right, rect.Bottom);
            Point bottomLeft = new(rect.Left, rect.Bottom);

            return SegmentosIntersectam(a, b, topLeft, topRight) ||
                   SegmentosIntersectam(a, b, topRight, bottomRight) ||
                   SegmentosIntersectam(a, b, bottomRight, bottomLeft) ||
                   SegmentosIntersectam(a, b, bottomLeft, topLeft);
        }

        private static bool SegmentosIntersectam(Point a, Point b, Point c, Point d)
        {
            double o1 = Orientacao(a, b, c);
            double o2 = Orientacao(a, b, d);
            double o3 = Orientacao(c, d, a);
            double o4 = Orientacao(c, d, b);

            if (SinaisOpostos(o1, o2) && SinaisOpostos(o3, o4))
                return true;

            return EhZero(o1) && EstaNoSegmento(a, c, b) ||
                   EhZero(o2) && EstaNoSegmento(a, d, b) ||
                   EhZero(o3) && EstaNoSegmento(c, a, d) ||
                   EhZero(o4) && EstaNoSegmento(c, b, d);
        }

        private static double Orientacao(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) -
                   (b.Y - a.Y) * (c.X - a.X);
        }

        private static bool SinaisOpostos(double a, double b)
        {
            return a > 0 && b < 0 || a < 0 && b > 0;
        }

        private static bool EhZero(double value)
        {
            return Math.Abs(value) < 0.000001;
        }

        private static bool EstaNoSegmento(Point a, Point p, Point b)
        {
            return p.X >= Math.Min(a.X, b.X) - 0.000001 &&
                   p.X <= Math.Max(a.X, b.X) + 0.000001 &&
                   p.Y >= Math.Min(a.Y, b.Y) - 0.000001 &&
                   p.Y <= Math.Max(a.Y, b.Y) + 0.000001;
        }
    }
}
