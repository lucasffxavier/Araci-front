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
        private readonly bool _modoSoMover;

        public SelecionarTool(EditorContext context, bool modoSoMover = false, bool mostrarHud = false)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _queries = context.SceneQueries;
            _selection = new SelectionController(context.Selection);
            _selectionBox = new SelectionBoxController(context.SelectionBox, _queries, context.Selection);
            _dragMove = new DragMoveController(context.Selection, context.Move, context.MoveHud, mostrarHud);
            _modoSoMover = modoSoMover;
        }

        public string Nome => "Selecionar";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _dragMove.IsActive || _selectionBox.IsActive;

        public void Ativar() { }

        public void Desativar()
        {
            Cancelar();
        }

        public void Cancelar()
        {
            _dragMove.Cancel();
            _selectionBox.Cancel();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            bool ctrl = inputState.IsControlPressed;

            var hit = vm ?? _queries.HitTest(position)?.Elemento;

            if (hit != null)
            {
                if (!_modoSoMover)
                    _selection.Select(hit, ctrl);

                _dragMove.Begin(position);
                return;
            }

            if (_modoSoMover)
                return;

            _selectionBox.Begin(position, ctrl);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
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
            if (_dragMove.IsActive)
                _dragMove.End();

            if (_selectionBox.IsActive)
                _selectionBox.End();
        }

        public void OnKeyDown(Key key) { }
    }
}
