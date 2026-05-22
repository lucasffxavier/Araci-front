using System.Windows;
using Araci.ViewModels;

namespace Araci.Core.SceneQueries
{
    public sealed class SceneHitResult
    {
        public SceneHitResult(ElementoViewModel elemento, Point point)
        {
            Elemento = elemento;
            Point = point;
        }

        public ElementoViewModel Elemento { get; }
        public Point Point { get; }
    }
}