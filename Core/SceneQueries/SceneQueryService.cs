using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Scenes;
using Araci.ViewModels;

namespace Araci.Core.SceneQueries
{
    public class SceneQueryService : ISceneQueryService
    {
        private readonly Scene _scene;

        public SceneQueryService(Scene scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        public SceneHitResult? HitTest(Point point)
        {
            for (int i = _scene.Elementos.Count - 1; i >= 0; i--)
            {
                var vm = _scene.Elementos[i];
                if (!vm.Bounds.Contains(point))
                    continue;

                return new SceneHitResult(vm, point);
            }

            return null;
        }

        public IEnumerable<ElementoViewModel> Query(Rect area)
        {
            return _scene.Elementos.Where(e => area.IntersectsWith(e.Bounds));
        }

        public IEnumerable<ElementoViewModel> Nearby(Point point, double radius)
        {
            double radius2 = radius * radius;

            foreach (var vm in _scene.Elementos)
            {
                var centro = vm.Centro;
                double dx = centro.X - point.X;
                double dy = centro.Y - point.Y;
                double dist2 = dx * dx + dy * dy;

                if (dist2 <= radius2)
                    yield return vm;
            }
        }
    }
}