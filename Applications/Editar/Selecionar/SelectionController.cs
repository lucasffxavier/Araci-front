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

        public bool Select(ElementoViewModel vm, bool ctrl, bool shift)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            if (shift)
            {
                if (EstaSelecionado(vm))
                    _selection.Deselecionar(vm);

                return false;
            }

            if (ctrl)
            {
                _selection.Toggle(vm);
                return true;
            }

            if (!EstaSelecionado(vm))
                _selection.Selecionar(vm);

            return true;
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