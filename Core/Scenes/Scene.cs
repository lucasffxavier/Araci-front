using System.Collections.ObjectModel;
using Araci.ViewModels;

namespace Araci.Core.Scenes
{
    public class Scene
    {
        public ObservableCollection<ElementoViewModel> Elementos { get; }

        public Scene()
        {
            Elementos = new ObservableCollection<ElementoViewModel>();
        }
    }
}