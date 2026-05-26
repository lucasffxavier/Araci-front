using System;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelectionController
    {
        private readonly SelectionService _selection;

        public SelectionController(SelectionService selection)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public void Select(ElementoViewModel vm, bool ctrl)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            if (ctrl)
            {
                _selection.Toggle(vm);
                return;
            }

            if (!EstaSelecionado(vm))
                _selection.Selecionar(vm);
        }

        private bool EstaSelecionado(ElementoViewModel vm)
        {
            foreach (ElementoViewModel selecionado in _selection.Selecionados)
            {
                if (ReferenceEquals(selecionado, vm))
                    return true;
            }

            return false;
        }
    }
}
