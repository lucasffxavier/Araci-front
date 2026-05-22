using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Scenes;
using Araci.Core.Spatial;
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
            return HitTest(point, 6);
        }

        public SceneHitResult? HitTest(Point point, double tolerance)
        {
            GarantirIndex();

            var candidatos = _index.Nearby(point, Math.Max(10, tolerance));

            foreach (var vm in candidatos.Reverse())
            {
                if (vm.Bounds.Contains(point))
                    return new SceneHitResult(vm, point);
            }

            return null;
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