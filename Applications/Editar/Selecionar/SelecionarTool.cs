using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Core.SceneQueries;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelecionarTool : ITool
    {
        private readonly ISceneQueryService _queries;
        private readonly SelectionController _selection;
        private readonly SelectionBoxController _selectionBox;
        private readonly DragMoveController _dragMove;
        private readonly CableVertexEditService _cableVertexEdit;
        private readonly BarraResizeService _barraResize;
        private readonly AlignmentGuideService _alignmentGuides;
        private readonly RotationService _rotation;
        private readonly bool _modoSoMover;

        public SelecionarTool(
            ISceneQueryService queries,
            SelectionService selection,
            SelectionBoxViewModel selectionBox,
            CableVertexEditService cableVertexEdit,
            BarraResizeService barraResize,
            MoveService move,
            MoveHudService moveHud,
            AlignmentGuideService alignmentGuides,
            MoveConstraintService moveConstraints,
            RotationService rotation,
            bool modoSoMover = false,
            bool mostrarHud = false)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _selection = new SelectionController(selection ?? throw new ArgumentNullException(nameof(selection)));
            _selectionBox = new SelectionBoxController(
                selectionBox ?? throw new ArgumentNullException(nameof(selectionBox)),
                _queries,
                selection);
            _cableVertexEdit = cableVertexEdit ?? throw new ArgumentNullException(nameof(cableVertexEdit));
            _barraResize = barraResize ?? throw new ArgumentNullException(nameof(barraResize));
            _alignmentGuides = alignmentGuides ?? throw new ArgumentNullException(nameof(alignmentGuides));
            _rotation = rotation ?? throw new ArgumentNullException(nameof(rotation));
            _dragMove = new DragMoveController(
                selection,
                move ?? throw new ArgumentNullException(nameof(move)),
                moveHud ?? throw new ArgumentNullException(nameof(moveHud)),
                _alignmentGuides,
                moveConstraints ?? throw new ArgumentNullException(nameof(moveConstraints)),
                mostrarHud);
            _modoSoMover = modoSoMover;
        }

        public string Nome => "Selecionar";
        public bool MantemBotaoAtivado => true;

        public bool IsBusy =>
            _barraResize.IsResizing ||
            _dragMove.IsActive ||
            _selectionBox.IsActive ||
            _cableVertexEdit.IsEditing;

        public void Ativar()
        {
            if (!_modoSoMover)
                _cableVertexEdit.Refresh();
        }

        public void Desativar()
        {
            Cancelar();
            _cableVertexEdit.Clear();
        }

        public void Cancelar()
        {
            _barraResize.Cancel();
            _dragMove.Cancel();
            _selectionBox.Cancel();
            _cableVertexEdit.Cancel();
            _cableVertexEdit.Refresh();
            _alignmentGuides.Limpar();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            bool ctrl = inputState.IsControlPressed;
            bool shift = inputState.IsShiftPressed;

            if (!_modoSoMover && _barraResize.TryBegin(position))
                return;

            if (!_modoSoMover && inputState.IsAltPressed && _cableVertexEdit.TryRemoveHandle(position))
                return;

            if (!_modoSoMover && ctrl && _cableVertexEdit.TryInsertVertex(position))
                return;

            if (!_modoSoMover && _cableVertexEdit.TryBegin(position))
                return;

            var hit = vm ?? _queries.HitTest(position)?.Elemento;

            if (hit != null)
            {
                if (!_modoSoMover)
                {
                    bool podeMover = _selection.Select(hit, ctrl, shift);
                    _cableVertexEdit.Clear();

                    if (!podeMover)
                        return;
                }

                _dragMove.Begin(position);
                return;
            }

            if (_modoSoMover)
                return;

            _selectionBox.Begin(position, ctrl);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (_barraResize.IsResizing)
            {
                _barraResize.Update(position);
                return;
            }

            if (_cableVertexEdit.IsEditing)
            {
                _cableVertexEdit.Update(position, inputState);
                return;
            }

            if (_dragMove.IsActive)
            {
                _dragMove.Update(position, inputState);
                return;
            }

            if (_selectionBox.IsActive)
                _selectionBox.Update(position);
        }

        public void OnMouseUp(Point position, ToolInputState inputState)
        {
            if (_barraResize.IsResizing)
            {
                _barraResize.End();
                _cableVertexEdit.Refresh();
                return;
            }

            if (_cableVertexEdit.IsEditing)
            {
                _cableVertexEdit.End();
                return;
            }

            if (_dragMove.IsActive)
            {
                _dragMove.End();
                _cableVertexEdit.Refresh();
            }

            if (_selectionBox.IsActive)
                _selectionBox.End();
        }

        public void OnKeyDown(Key key)
        {
            if (!_modoSoMover && key == Key.Space)
            {
                _rotation.RotateSelectionClockwise();
                return;
            }

            if (!_modoSoMover && key == Key.Delete)
                _cableVertexEdit.TryRemoveActive();
        }
    }
}
