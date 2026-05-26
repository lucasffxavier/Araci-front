using System;
using System.Linq;
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
        private readonly EditorContext _context;
        private readonly ISceneQueryService _queries;
        private readonly SelectionBoxController _selectionBox;
        private readonly bool _modoSoMover;
        private readonly bool _mostrarHud;

        private bool _arrastandoElementos;

        private Point _ultimoPontoMouse;
        private Point _pontoInicialArrasto;

        public SelecionarTool(EditorContext context, bool modoSoMover = false, bool mostrarHud = false)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queries = context.SceneQueries;
            _selectionBox = new SelectionBoxController(context.SelectionBox, _queries, context.Selection);
            _modoSoMover = modoSoMover;
            _mostrarHud = mostrarHud;
        }

        public string Nome => "Selecionar";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _arrastandoElementos || _selectionBox.IsActive;

        public void Ativar() { }

        public void Desativar()
        {
            Cancelar();
        }

        public void Cancelar()
        {
            _arrastandoElementos = false;
            _selectionBox.Cancel();
            _context.Move.AbortMove();
            _context.MoveHud.Visivel = false;
            _context.MoveHud.Reset();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            bool ctrl = inputState.IsControlPressed;

            var hit = vm ?? _queries.HitTest(position)?.Elemento;

            if (hit != null)
            {
                IniciarSelecaoElemento(hit, ctrl);
                IniciarMovimento(position);
                return;
            }

            if (_modoSoMover)
                return;

            _selectionBox.Begin(position, ctrl);
        }

        public void OnMouseMove(Point position)
        {
            if (_arrastandoElementos)
            {
                AtualizarMovimento(position);
                return;
            }

            if (_selectionBox.IsActive)
                _selectionBox.Update(position);
        }

        public void OnMouseUp(Point position)
        {
            if (_arrastandoElementos)
                FinalizarMovimento();

            if (_selectionBox.IsActive)
                _selectionBox.End();

            _arrastandoElementos = false;
        }

        public void OnKeyDown(Key key) { }

        private void IniciarSelecaoElemento(ElementoViewModel vm, bool ctrl)
        {
            if (_modoSoMover)
                return;

            if (ctrl)
            {
                _context.Selection.Toggle(vm);
                return;
            }

            if (!_context.Selection.Selecionados.Contains(vm))
                _context.Selection.Selecionar(vm);
        }

        private void IniciarMovimento(Point position)
        {
            _arrastandoElementos = true;
            _ultimoPontoMouse = position;
            _pontoInicialArrasto = position;

            _context.Move.BeginMove(_context.Selection.Selecionados);

            if (!_mostrarHud)
                return;

            var hud = _context.MoveHud;
            hud.Reset();
            hud.AtualizarPosicao(CalcularBoundsSelecionados());
            hud.Visivel = true;
        }

        private void AtualizarMovimento(Point position)
        {
            Vector delta = position - _ultimoPontoMouse;

            if (delta.X != 0 || delta.Y != 0)
            {
                foreach (var item in _context.Selection.Selecionados.ToList())
                    _context.Move.MoverVisual(item, delta);
            }

            if (_mostrarHud)
                AtualizarHud(position);

            _ultimoPontoMouse = position;
        }

        private void AtualizarHud(Point position)
        {
            Vector delta = position - _pontoInicialArrasto;

            var hud = _context.MoveHud;
            hud.DeltaX = delta.X;
            hud.DeltaY = delta.Y;
            hud.AtualizarPosicao(CalcularBoundsSelecionados());
        }

        private void FinalizarMovimento()
        {
            _context.Move.EndMove(_context.Selection.Selecionados.ToList());
            _context.MoveHud.Visivel = false;
            _context.MoveHud.Reset();
        }

        private Rect CalcularBoundsSelecionados()
        {
            var items = _context.Selection.Selecionados;

            if (items.Count == 0)
                return Rect.Empty;

            double minX = items.Min(i => i.Bounds.Left);
            double minY = items.Min(i => i.Bounds.Top);
            double maxX = items.Max(i => i.Bounds.Right);
            double maxY = items.Max(i => i.Bounds.Bottom);

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
