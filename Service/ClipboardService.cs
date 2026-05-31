using System;
using System.Collections.Generic;
using Araci.Applications.UseCases.Editar;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ClipboardService
    {
        private readonly CopiarElementosUseCase _copiarElementos;
        private readonly ColarElementosUseCase _colarElementos;
        private readonly SelectionService _selection;
        private readonly Func<ViewportService?> _viewportProvider;
        private readonly ISceneQueryService _sceneQueries;
        private readonly CableVertexEditService _cableVertexEdit;

        public ClipboardService(
            CopiarElementosUseCase copiarElementos,
            ColarElementosUseCase colarElementos,
            SelectionService selection,
            Func<ViewportService?> viewportProvider,
            ISceneQueryService sceneQueries,
            CableVertexEditService cableVertexEdit)
        {
            _copiarElementos = copiarElementos ?? throw new ArgumentNullException(nameof(copiarElementos));
            _colarElementos = colarElementos ?? throw new ArgumentNullException(nameof(colarElementos));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _viewportProvider = viewportProvider ?? throw new ArgumentNullException(nameof(viewportProvider));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _cableVertexEdit = cableVertexEdit ?? throw new ArgumentNullException(nameof(cableVertexEdit));
        }

        public void CopiarSelecionados()
        {
            _copiarElementos.Executar(_selection.Selecionados);
        }

        public void Colar()
        {
            IReadOnlyList<Elemento> colados = _colarElementos.Executar();

            if (colados.Count == 0)
                return;

            _selection.Limpar();
            ViewportService? viewport = _viewportProvider();

            foreach (Elemento elemento in colados)
            {
                ElementoViewModel? vm = viewport?.ObterViewModel(elemento);
                if (vm != null)
                    _selection.Selecionar(vm, true);
            }

            _sceneQueries.Invalidate();
            _cableVertexEdit.Refresh();
        }
    }
}
