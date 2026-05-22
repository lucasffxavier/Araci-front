using System.Collections.Generic;
using System.Windows;
using Araci.ViewModels;

namespace Araci.Core.Spatial
{
    public interface ISpatialIndex
    {
        void Build(IEnumerable<ElementoViewModel> elementos);
        void Add(ElementoViewModel elemento);
        void Remove(ElementoViewModel elemento);
        void Update(ElementoViewModel elemento);

        IEnumerable<ElementoViewModel> Query(Rect area);
        IEnumerable<ElementoViewModel> Nearby(Point point, double radius);
    }
}