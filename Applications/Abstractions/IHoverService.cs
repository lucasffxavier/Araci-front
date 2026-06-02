using System.Windows;
using Araci.Services.Interaction;

namespace Araci.Applications.Abstractions
{
    public interface IHoverService
    {
        void Update(Point worldPosition);
        void Clear();
    }
}
