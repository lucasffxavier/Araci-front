using System.Windows;

namespace Araci.Applications.Abstractions
{
    public interface IHoverService
    {
        void Update(Point worldPosition);
        void Clear();
    }
}
