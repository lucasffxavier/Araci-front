using System;
using System.Linq;
using System.Windows;
using Araci.Applications.Editar.Base;
using Araci.Services;

namespace Araci.Applications.Editar.Selecionar
{
    public class DragMoveController
    {
        private readonly SelectionService _selection;
        private readonly MoveService _move;
        private readonly MoveHudService _hud;
        private readonly bool _mostrarHud;

        private Point _ultimoPontoMouse;
        private Point _ultimoPontoEfetivo;
        private Point _pontoInicialArrasto;
        private OrthogonalAxis? _eixoTravado;

        public DragMoveController(
            SelectionService selection,
            MoveService move,
            MoveHudService hud,
            bool mostrarHud)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _move = move ?? throw new ArgumentNullException(nameof(move));
            _hud = hud ?? throw new ArgumentNullException(nameof(hud));
            _mostrarHud = mostrarHud;
        }

        public bool IsActive { get; private set; }

        public void Begin(Point position)
        {
            IsActive = true;
            _ultimoPontoMouse = position;
            _ultimoPontoEfetivo = position;
            _pontoInicialArrasto = position;
            _eixoTravado = null;

            _move.BeginMove(_selection.Selecionados);

            if (!_mostrarHud)
                return;

            _hud.Reset();
            _hud.AtualizarPosicao(CalcularBoundsSelecionados());
            _hud.Visivel = true;
        }

        public void Update(Point position, ToolInputState inputState)
        {
            if (!IsActive)
                return;

            Point pontoEfetivo = CalcularPontoEfetivo(position, inputState);

            Vector delta = pontoEfetivo - _ultimoPontoEfetivo;

            if (delta.X != 0 || delta.Y != 0)
            {
                foreach (var item in _selection.Selecionados.ToList())
                    _move.MoverVisual(item, delta);
            }

            if (_mostrarHud)
                AtualizarHud(pontoEfetivo);

            _ultimoPontoMouse = position;
            _ultimoPontoEfetivo = pontoEfetivo;
        }

        public void End()
        {
            if (!IsActive)
                return;

            _move.EndMove(_selection.Selecionados.ToList());
            IsActive = false;
            _eixoTravado = null;
            LimparHud();
        }

        public void Cancel()
        {
            IsActive = false;
            _eixoTravado = null;
            _move.AbortMove();
            LimparHud();
        }

        private void AtualizarHud(Point position)
        {
            Vector delta = position - _pontoInicialArrasto;

            _hud.DeltaX = delta.X;
            _hud.DeltaY = delta.Y;
            _hud.AtualizarPosicao(CalcularBoundsSelecionados());
        }

        private Point CalcularPontoEfetivo(Point position, ToolInputState inputState)
        {
            if (!inputState.IsShiftPressed)
            {
                _eixoTravado = null;
                return position;
            }

            return AplicarTravaOrtogonal(position);
        }

        private Point AplicarTravaOrtogonal(Point position)
        {
            Vector total = position - _pontoInicialArrasto;

            if (!_eixoTravado.HasValue)
            {
                if (Math.Abs(total.X) < 0.0001 && Math.Abs(total.Y) < 0.0001)
                    return _ultimoPontoEfetivo;

                _eixoTravado = Math.Abs(total.X) >= Math.Abs(total.Y)
                    ? OrthogonalAxis.Horizontal
                    : OrthogonalAxis.Vertical;
            }

            return _eixoTravado == OrthogonalAxis.Horizontal
                ? new Point(position.X, _pontoInicialArrasto.Y)
                : new Point(_pontoInicialArrasto.X, position.Y);
        }

        private void LimparHud()
        {
            _hud.Visivel = false;
            _hud.Reset();
        }

        private Rect CalcularBoundsSelecionados()
        {
            var items = _selection.Selecionados;

            if (items.Count == 0)
                return Rect.Empty;

            double minX = items.Min(i => i.Bounds.Left);
            double minY = items.Min(i => i.Bounds.Top);
            double maxX = items.Max(i => i.Bounds.Right);
            double maxY = items.Max(i => i.Bounds.Bottom);

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private enum OrthogonalAxis
        {
            Horizontal,
            Vertical
        }
    }
}
