using System.Collections.Generic;
using Araci.ViewModels;
using Araci.Services.Editing;

namespace Araci.Applications.Abstractions
{
    public interface ISelectionService
    {
        IReadOnlyList<ElementoViewModel> Selecionados { get; }
        void Deselecionar(ElementoViewModel vm);
        void Limpar();
    }
}
