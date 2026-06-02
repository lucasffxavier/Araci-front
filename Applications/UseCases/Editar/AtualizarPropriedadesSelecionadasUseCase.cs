using System;
using Araci.Services.Editing;

namespace Araci.Applications.UseCases.Editar
{
    public class AtualizarPropriedadesSelecionadasUseCase
    {
        private readonly SelectionService _selection;

        public AtualizarPropriedadesSelecionadasUseCase(SelectionService selection)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public void Executar()
        {
            _selection.RefreshProperties();
        }
    }
}
