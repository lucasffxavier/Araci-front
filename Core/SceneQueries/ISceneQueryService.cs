using System.Collections.Generic;
using System.Windows;
using Araci.ViewModels;

namespace Araci.Core.SceneQueries
{
    public interface ISceneQueryService
    {
        SceneHitResult? HitTest(Point point);
        SceneHitResult? HitTest(Point point, double tolerance);

        IEnumerable<ElementoViewModel> Query(Rect area);

        IEnumerable<ElementoViewModel> Nearby(Point point, double radius);
    }
}