using Araci.Models;
using Araci.ViewModels;

namespace Araci.Applications.Abstractions
{
    public interface IElementViewModelFactory
    {
        ElementoViewModel? CreateViewModel(Elemento modelo);
        TViewModel CreateViewModel<TViewModel>(string kind) where TViewModel : ElementoViewModel;
    }
}
