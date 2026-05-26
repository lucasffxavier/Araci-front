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

            double effectiveTolerance = Math.Max(6, tolerance);
            var candidatos = _index.Nearby(point, Math.Max(10, effectiveTolerance))
                .Where(e => !e.IsPreview)
                .ToList();
            HitCandidate? melhor = null;

            for (int i = 0; i < candidatos.Count; i++)
            {
                HitCandidate? candidato = CriarCandidato(candidatos[i], point, effectiveTolerance, i);

                if (candidato.HasValue && EhMelhor(candidato.Value, melhor))
                    melhor = candidato.Value;
            }

            return melhor.HasValue
                ? new SceneHitResult(melhor.Value.Elemento, point)
                : null;
        }

        public IEnumerable<ElementoViewModel> Query(Rect area)
        {
            GarantirIndex();
            return _index.Query(area).Where(e => !e.IsPreview);
        }

        public IEnumerable<ElementoViewModel> Nearby(Point point, double radius)
        {
            GarantirIndex();
            return _index.Nearby(point, radius).Where(e => !e.IsPreview);
        }

        private void GarantirIndex()
        {
            if (_indexValido)
                return;

            _index.Build(_scene.Elementos);
            _indexValido = true;
        }

        private static HitCandidate? CriarCandidato(ElementoViewModel vm, Point point, double tolerance, int order)
        {
            if (vm is CaboViewModel cabo)
                return CriarCandidatoCabo(cabo, point, tolerance, order);

            return vm.Bounds.Contains(point)
                ? new HitCandidate(vm, 0, 0, order)
                : null;
        }

        private static HitCandidate? CriarCandidatoCabo(CaboViewModel cabo, Point point, double tolerance, int order)
        {
            var vertices = cabo.Cabo.Vertices;

            if (vertices.Count < 2)
            {
                return cabo.Bounds.Contains(point)
                    ? new HitCandidate(cabo, 0, 3, order)
                    : null;
            }

            double distancia = MenorDistanciaCabo(cabo, point);

            return distancia <= tolerance
                ? new HitCandidate(cabo, distancia, 2, order)
                : null;
        }

        private static bool EhMelhor(HitCandidate candidato, HitCandidate? atual)
        {
            if (!atual.HasValue)
                return true;

            HitCandidate existente = atual.Value;

            if (candidato.Priority != existente.Priority)
                return candidato.Priority < existente.Priority;

            if (Math.Abs(candidato.Distance - existente.Distance) > 0.000001)
                return candidato.Distance < existente.Distance;

            return candidato.Order > existente.Order;
        }

        private static double MenorDistanciaCabo(CaboViewModel cabo, Point point)
        {
            var vertices = cabo.Cabo.Vertices;
            double menor = double.MaxValue;

            for (int i = 0; i < vertices.Count - 1; i++)
                menor = Math.Min(menor, DistanciaPontoSegmento(point, vertices[i], vertices[i + 1]));

            return menor;
        }

        private static double DistanciaPontoSegmento(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;

            if (lengthSquared <= double.Epsilon)
                return Distancia(p, a);

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));

            var projection = new Point(
                a.X + t * dx,
                a.Y + t * dy);

            return Distancia(p, projection);
        }

        private static double Distancia(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private readonly struct HitCandidate
        {
            public HitCandidate(ElementoViewModel elemento, double distance, int priority, int order)
            {
                Elemento = elemento;
                Distance = distance;
                Priority = priority;
                Order = order;
            }

            public ElementoViewModel Elemento { get; }

            public double Distance { get; }

            public int Priority { get; }

            public int Order { get; }
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
