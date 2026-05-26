using System;
using System.Linq;
using System.Windows;
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
        private Point _pontoInicialArrasto;

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
            _pontoInicialArrasto = position;

            _move.BeginMove(_selection.Selecionados);

            if (!_mostrarHud)
                return;

            _hud.Reset();
            _hud.AtualizarPosicao(CalcularBoundsSelecionados());
            _hud.Visivel = true;
        }

        public void Update(Point position)
        {
            if (!IsActive)
                return;

            Vector delta = position - _ultimoPontoMouse;

            if (delta.X != 0 || delta.Y != 0)
            {
                foreach (var item in _selection.Selecionados.ToList())
                    _move.MoverVisual(item, delta);
            }

            if (_mostrarHud)
                AtualizarHud(position);

            _ultimoPontoMouse = position;
        }

        public void End()
        {
            if (!IsActive)
                return;

            _move.EndMove(_selection.Selecionados.ToList());
            IsActive = false;
            LimparHud();
        }

        public void Cancel()
        {
            IsActive = false;
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
    }
}
