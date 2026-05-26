using System;
using System.Windows;
using Araci.Core.SceneQueries;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelectionBoxController
    {
        private readonly SelectionBoxViewModel _selectionBox;
        private readonly ISceneQueryService _queries;
        private readonly SelectionService _selection;

        private Point _inicio;
        private bool _adicionarAoExistente;

        public SelectionBoxController(
            SelectionBoxViewModel selectionBox,
            ISceneQueryService queries,
            SelectionService selection)
        {
            _selectionBox = selectionBox ?? throw new ArgumentNullException(nameof(selectionBox));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public bool IsActive { get; private set; }

        public void Begin(Point position, bool adicionarAoExistente)
        {
            _adicionarAoExistente = adicionarAoExistente;

            if (!_adicionarAoExistente)
                _selection.Limpar();

            _inicio = position;
            IsActive = true;

            _selectionBox.Visivel = true;
            _selectionBox.Atualizar(position, position);
        }

        public void Update(Point position)
        {
            if (!IsActive)
                return;

            _selectionBox.Atualizar(_inicio, position);
        }

        public void End()
        {
            if (!IsActive)
                return;

            Rect bounds = _selectionBox.Bounds;

            foreach (ElementoViewModel item in _queries.Query(bounds))
                _selection.Selecionar(item, true);

            Cancel();
        }

        public void Cancel()
        {
            IsActive = false;
            _selectionBox.Visivel = false;
        }
    }
}
