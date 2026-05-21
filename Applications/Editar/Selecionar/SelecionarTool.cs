using System.Linq;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelecionarTool : ITool
    {
        private readonly bool _modoSoMover;
        private readonly bool _mostrarHud;
        private readonly EditorContext _context;

        private bool _arrastandoElementos;
        private bool _selecionandoJanela;

        private Point _inicioJanela;
        private Point _ultimoPontoMouse;
        private Point _pontoInicialArrasto;

        public string Nome => "Selecionar";
        public bool MantemBotaoAtivado => true;

        public SelecionarTool(
            EditorContext context,
            bool modoSoMover = false,
            bool mostrarHud = false)
        {
            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));

            _modoSoMover = modoSoMover;
            _mostrarHud = mostrarHud;
        }

        public void Ativar() { }

        public void Desativar()
        {
            _arrastandoElementos = false;
            _selecionandoJanela = false;

            _context.SelectionBox.Visivel = false;

            _context.MoveHud.Visivel = false;
            _context.MoveHud.Reset();
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position,
            ToolInputState inputState)
        {
            Point worldPosition = ScreenToWorld(position);

            bool ctrl = inputState.IsControlPressed;

            if (vm != null)
            {
                if (!_modoSoMover)
                {
                    if (ctrl)
                        _context.Selection.Toggle(vm);
                    else if (!_context.Selection.Selecionados.Contains(vm))
                        _context.Selection.Selecionar(vm);
                }

                _arrastandoElementos = true;

                _ultimoPontoMouse = worldPosition;
                _pontoInicialArrasto = worldPosition;

                _context.Move.BeginMove(
                    _context.Selection.Selecionados);

                if (_mostrarHud)
                {
                    var hud = _context.MoveHud;

                    hud.Reset();

                    var bounds = CalcularBoundsSelecionados();

                    hud.AtualizarPosicao(bounds);
                    hud.Visivel = true;
                }

                return;
            }

            if (_modoSoMover)
                return;

            if (!ctrl)
                _context.Selection.Limpar();

            _inicioJanela = worldPosition;

            _selecionandoJanela = true;

            _context.SelectionBox.Visivel = true;

            _context.SelectionBox.Atualizar(
                worldPosition,
                worldPosition);
        }

        public void OnMouseMove(Point position)
        {
            Point worldPosition = ScreenToWorld(position);

            if (_arrastandoElementos)
            {
                Vector delta = worldPosition - _ultimoPontoMouse;

                if (delta.X != 0 || delta.Y != 0)
                {
                    foreach (var item in _context.Selection.Selecionados.ToList())
                        _context.Move.MoverVisual(item, delta);
                }

                if (_mostrarHud)
                {
                    var deltaTotal = worldPosition - _pontoInicialArrasto;

                    var hud = _context.MoveHud;

                    hud.DeltaX = deltaTotal.X;
                    hud.DeltaY = deltaTotal.Y;

                    var bounds = CalcularBoundsSelecionados();

                    hud.AtualizarPosicao(bounds);
                }

                _ultimoPontoMouse = worldPosition;

                return;
            }

            if (_selecionandoJanela)
            {
                _context.SelectionBox.Atualizar(
                    _inicioJanela,
                    worldPosition);
            }
        }

        public void OnMouseUp(Point position)
        {
            if (_arrastandoElementos)
            {
                _context.Move.EndMove(
                    _context.Selection.Selecionados.ToList());

                _context.MoveHud.Visivel = false;
                _context.MoveHud.Reset();
            }

            if (_selecionandoJanela)
            {
                var rect = _context.SelectionBox.Bounds;

                if (_context.Viewport != null)
                {
                    foreach (var item in _context.Viewport.Elementos)
                    {
                        if (rect.IntersectsWith(item.Bounds))
                            _context.Selection.Selecionar(item, true);
                    }
                }

                _context.SelectionBox.Visivel = false;
            }

            _arrastandoElementos = false;
            _selecionandoJanela = false;
        }

        public void OnKeyDown(Key key) { }

        private Rect CalcularBoundsSelecionados()
        {
            var items = _context.Selection.Selecionados;

            if (items.Count == 0)
                return Rect.Empty;

            double minX = items.Min(i => i.Bounds.Left);
            double minY = items.Min(i => i.Bounds.Top);
            double maxX = items.Max(i => i.Bounds.Right);
            double maxY = items.Max(i => i.Bounds.Bottom);

            return new Rect(
                minX,
                minY,
                maxX - minX,
                maxY - minY);
        }

        private Point ScreenToWorld(Point screenPosition)
        {
            return _context.Viewport
                ?.ScreenToWorld(screenPosition)
                ?? screenPosition;
        }
    }
}