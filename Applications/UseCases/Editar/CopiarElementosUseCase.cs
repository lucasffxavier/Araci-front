using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Applications.UseCases.Editar
{
    public class CopiarElementosUseCase
    {
        private readonly List<Elemento> _copiados = new();

        public IReadOnlyList<Elemento> Copiados => _copiados;
        public bool TemElementos => _copiados.Count > 0;

        public void Executar(IEnumerable<ElementoViewModel> selecionados)
        {
            ArgumentNullException.ThrowIfNull(selecionados);
            Executar(selecionados.Select(vm => vm.Modelo));
        }

        public void Executar(IEnumerable<Elemento> elementos)
        {
            ArgumentNullException.ThrowIfNull(elementos);
            _copiados.Clear();

            foreach (Elemento elemento in elementos)
                _copiados.Add(elemento.Clonar());
        }
    }
}
