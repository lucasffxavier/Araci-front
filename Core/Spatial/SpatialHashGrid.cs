using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.ViewModels;

namespace Araci.Core.Spatial
{
    public class SpatialHashGrid : ISpatialIndex
    {
        private readonly Dictionary<(int, int), List<ElementoViewModel>> _cells = new();
        private readonly double _cellSize;

        public SpatialHashGrid(double cellSize = 100)
        {
            _cellSize = cellSize;
        }

        public void Build(IEnumerable<ElementoViewModel> elementos)
        {
            _cells.Clear();
            foreach (var e in elementos)
                Add(e);
        }

        public void Add(ElementoViewModel elemento)
        {
            foreach (var cell in GetCells(elemento.Bounds))
            {
                if (!_cells.TryGetValue(cell, out var list))
                {
                    list = new List<ElementoViewModel>();
                    _cells[cell] = list;
                }
                list.Add(elemento);
            }
        }

        public void Remove(ElementoViewModel elemento)
        {
            foreach (var cell in GetCells(elemento.Bounds))
            {
                if (_cells.TryGetValue(cell, out var list))
                    list.Remove(elemento);
            }
        }

        public void Update(ElementoViewModel elemento)
        {
            Remove(elemento);
            Add(elemento);
        }

        public IEnumerable<ElementoViewModel> Query(Rect area)
        {
            var visited = new HashSet<ElementoViewModel>();

            foreach (var cell in GetCells(area))
            {
                if (!_cells.TryGetValue(cell, out var list))
                    continue;

                foreach (var e in list)
                {
                    if (visited.Add(e) && area.IntersectsWith(e.Bounds))
                        yield return e;
                }
            }
        }

        public IEnumerable<ElementoViewModel> Nearby(Point point, double radius)
        {
            var area = new Rect(
                point.X - radius,
                point.Y - radius,
                radius * 2,
                radius * 2);

            foreach (var e in Query(area))
            {
                var c = e.Centro;
                double dx = c.X - point.X;
                double dy = c.Y - point.Y;

                if (dx * dx + dy * dy <= radius * radius)
                    yield return e;
            }
        }

        private IEnumerable<(int, int)> GetCells(Rect bounds)
        {
            int minX = (int)Math.Floor(bounds.Left / _cellSize);
            int minY = (int)Math.Floor(bounds.Top / _cellSize);
            int maxX = (int)Math.Floor(bounds.Right / _cellSize);
            int maxY = (int)Math.Floor(bounds.Bottom / _cellSize);

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    yield return (x, y);
        }
    }
}