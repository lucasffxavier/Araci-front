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
        private readonly bool _modoSoMover;

        public SelecionarTool(EditorContext context, bool modoSoMover = false, bool mostrarHud = false)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _queries = context.SceneQueries;
            _selection = new SelectionController(context.Selection);
            _selectionBox = new SelectionBoxController(context.SelectionBox, _queries, context.Selection);
            _cableVertexEdit = context.CableVertexEdit;
            _dragMove = new DragMoveController(
                context.Selection,
                context.Move,
                context.MoveHud,
                context.MoveConstraints,
                mostrarHud);
            _modoSoMover = modoSoMover;
        }

        public string Nome => "Selecionar";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy =>
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
            _dragMove.Cancel();
            _selectionBox.Cancel();
            _cableVertexEdit.Cancel();
            _cableVertexEdit.Refresh();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            bool ctrl = inputState.IsControlPressed;

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
                    _selection.Select(hit, ctrl);

                _cableVertexEdit.Clear();
                _dragMove.Begin(position);
                return;
            }

            if (_modoSoMover)
                return;

            _selectionBox.Begin(position, ctrl);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
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
            if (!_modoSoMover && key == Key.Delete)
                _cableVertexEdit.TryRemoveActive();
        }
    }
}
