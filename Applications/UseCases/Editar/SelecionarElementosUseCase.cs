using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Services.Editing;
using Araci.ViewModels;

namespace Araci.Applications.UseCases.Editar
{
    public class SelecionarElementosUseCase
    {
        private readonly SelectionService _selection;

        public SelecionarElementosUseCase(SelectionService selection)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public void Selecionar(ElementoViewModel elemento)
        {
            _selection.Selecionar(elemento);
        }

        public void Selecionar(IEnumerable<ElementoViewModel> elementos)
        {
            if (elementos == null)
                throw new ArgumentNullException(nameof(elementos));

            var lista = elementos.Where(e => e != null).ToList();

            _selection.Limpar();

            foreach (ElementoViewModel elemento in lista)
                _selection.Selecionar(elemento, true);
        }

        public void Adicionar(ElementoViewModel elemento)
        {
            _selection.Selecionar(elemento, true);
        }

        public void Remover(ElementoViewModel elemento)
        {
            _selection.Deselecionar(elemento);
        }

        public void Limpar()
        {
            _selection.Limpar();
        }

        public void Alternar(ElementoViewModel elemento)
        {
            _selection.Toggle(elemento);
        }
    }
}
