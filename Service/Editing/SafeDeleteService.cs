using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Selecionar;
using Araci.Applications.UseCases.Editar;
using Araci.Core.SceneQueries;
using Araci.ViewModels;
using Araci.Services;
using Araci.Services.Interaction;

namespace Araci.Services.Editing
{
    public class SafeDeleteService : ISafeDeleteService
    {
        private readonly SelectionService _selection;
        private readonly CableVertexEditService _cableVertexEdit;
        private readonly ExcluirElementoUseCase _excluirElemento;
        private readonly HoverService _hover;
        private readonly TerminalSnapState _terminalSnap;
        private readonly ISceneQueryService _sceneQueries;

        public SafeDeleteService(
            SelectionService selection,
            CableVertexEditService cableVertexEdit,
            ExcluirElementoUseCase excluirElemento,
            HoverService hover,
            TerminalSnapState terminalSnap,
            ISceneQueryService sceneQueries)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _cableVertexEdit = cableVertexEdit ?? throw new ArgumentNullException(nameof(cableVertexEdit));
            _excluirElemento = excluirElemento ?? throw new ArgumentNullException(nameof(excluirElemento));
            _hover = hover ?? throw new ArgumentNullException(nameof(hover));
            _terminalSnap = terminalSnap ?? throw new ArgumentNullException(nameof(terminalSnap));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
        }

        public bool DeleteActiveHandleOrSelection()
        {
            if (_cableVertexEdit.TryRemoveActive())
                return true;

            return DeleteSelection();
        }

        public bool DeleteSelection()
        {
            var selecionados = _selection.Selecionados.ToList();

            if (selecionados.Count == 0)
                return false;

            if (!_excluirElemento.Executar(selecionados))
                return false;

            LimparEstadoVisual();
            return true;
        }

        private void LimparEstadoVisual()
        {
            _selection.Limpar();
            _cableVertexEdit.Clear();
            _hover.Clear();
            _terminalSnap.Limpar();
            _sceneQueries.Invalidate();
        }
    }
}
