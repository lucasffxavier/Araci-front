using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
        private readonly HashSet<ElementoViewModel> _observados = new();
        private bool _indexValido;

        public SceneQueryService(Scene scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _index = new SpatialHashGrid();

            _scene.Elementos.CollectionChanged += OnElementosChanged;

            foreach (ElementoViewModel elemento in _scene.Elementos)
                Observar(elemento);

            Invalidate();
        }

        public void Invalidate()
        {
            _indexValido = false;
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

        private void GarantirIndex()
        {
            if (_indexValido)
                return;

            _index.Build(_scene.Elementos);
            _indexValido = true;
        }

        private void OnElementosChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                ReobservarTodos();

            if (e.OldItems != null)
            {
                foreach (ElementoViewModel vm in e.OldItems)
                    Desobservar(vm);
            }

            if (e.NewItems != null)
            {
                foreach (ElementoViewModel vm in e.NewItems)
                    Observar(vm);
            }

            Invalidate();
        }

        private void Observar(ElementoViewModel vm)
        {
            if (!_observados.Add(vm))
                return;

            vm.PropertyChanged += OnElementoPropertyChanged;
        }

        private void Desobservar(ElementoViewModel vm)
        {
            if (!_observados.Remove(vm))
                return;

            vm.PropertyChanged -= OnElementoPropertyChanged;
        }

        private void ReobservarTodos()
        {
            foreach (ElementoViewModel vm in _observados.ToList())
                Desobservar(vm);

            foreach (ElementoViewModel vm in _scene.Elementos)
                Observar(vm);
        }

        private void OnElementoPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName is nameof(ElementoViewModel.Bounds) or
                    nameof(ElementoViewModel.X) or
                    nameof(ElementoViewModel.Y) or
                    nameof(ElementoViewModel.WorldX) or
                    nameof(ElementoViewModel.WorldY) or
                    nameof(ElementoViewModel.Largura) or
                    nameof(ElementoViewModel.Altura) or
                    nameof(ElementoViewModel.Centro) or
                    nameof(ElementoViewModel.RenderData))
            {
                Invalidate();
            }
        }
    }
}
