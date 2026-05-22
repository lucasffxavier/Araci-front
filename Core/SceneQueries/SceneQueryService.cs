using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Scenes;
using Araci.Core.Spatial;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Core.SceneQueries
{
    public class SceneQueryService : ISceneQueryService
    {
        private readonly Scene _scene;
        private readonly ISpatialIndex _index;
        private bool _indexValido;

        public SceneQueryService(Scene scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _index = new SpatialHashGrid();
            _indexValido = false;

            _scene.Elementos.CollectionChanged += (_, __) => _indexValido = false;
        }

        private void GarantirIndex()
        {
            if (_indexValido)
                return;

            _index.Build(_scene.Elementos);
            _indexValido = true;
        }

        public SceneHitResult? HitTest(Point point)
        {
            return HitTest(point, 6); // tolerância padrão
        }

        public SceneHitResult? HitTest(Point point, double tolerance)
        {
            GarantirIndex();

            var candidatos = _index.Nearby(point, Math.Max(10, tolerance));

            foreach (var vm in candidatos.Reverse())
            {
                // 1. Equipamentos (bounding box)
                if (vm.Bounds.Contains(point))
                    return new SceneHitResult(vm, point);

                // 2. Cabos (teste por distância)
                if (vm.Modelo is Cabo cabo)
                {
                    if (HitTestCabo(cabo, point, tolerance))
                        return new SceneHitResult(vm, point);
                }
            }

            return null;
        }

        private bool HitTestCabo(Cabo cabo, Point p, double tolerance)
        {
            var pontos = cabo.Vertices;

            if (pontos.Count < 2)
                return false;

            double tol2 = tolerance * tolerance;

            for (int i = 0; i < pontos.Count - 1; i++)
            {
                if (DistanciaPontoSegmento2(p, pontos[i], pontos[i + 1]) <= tol2)
                    return true;
            }

            return false;
        }

        private static double DistanciaPontoSegmento2(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;

            if (dx == 0 && dy == 0)
                return Dist2(p, a);

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;

            return Dist2(p, new Point(projX, projY));
        }

        private static double Dist2(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return dx * dx + dy * dy;
        }

        public IEnumerable<ElementoViewModel> Query(Rect area)
        {
            GarantirIndex();
            return _index.Query(area);
        }

        public IEnumerable<ElementoViewModel> Nearby(Point point, double radius)
        {
            GarantirIndex();
            return _index.Nearby(point, radius);
        }
    }
}