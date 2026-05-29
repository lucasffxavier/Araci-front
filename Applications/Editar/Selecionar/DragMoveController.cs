using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class DragMoveController
    {
        private readonly SelectionService _selection;
        private readonly MoveService _move;
        private readonly MoveHudService _moveHud;
        private readonly AlignmentGuideService _alignmentGuides;
        private readonly MoveConstraintService _constraints;
        private readonly bool _mostrarHud;
        private readonly List<ElementoViewModel> _elementos = new();
        private Point _start;
        private Vector _deltaAplicado;

        public DragMoveController(SelectionService selection, MoveService move, MoveHudService moveHud, AlignmentGuideService alignmentGuides, MoveConstraintService constraints, bool mostrarHud)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _move = move ?? throw new ArgumentNullException(nameof(move));
            _moveHud = moveHud ?? throw new ArgumentNullException(nameof(moveHud));
            _alignmentGuides = alignmentGuides ?? throw new ArgumentNullException(nameof(alignmentGuides));
            _constraints = constraints ?? throw new ArgumentNullException(nameof(constraints));
            _mostrarHud = mostrarHud;
        }

        public bool IsActive { get; private set; }

        public void Begin(Point position)
        {
            _elementos.Clear();
            _elementos.AddRange(_selection.Selecionados.Distinct().Where(e => !e.IsPreview));

            if (_elementos.Count == 0)
                return;

            _start = position;
            _deltaAplicado = default;
            IsActive = true;
            _constraints.Begin(position);
            _move.BeginMove(_elementos);
            _alignmentGuides.Limpar();

            if (_mostrarHud)
            {
                _moveHud.Reset();
                _moveHud.Visivel = true;
                AtualizarHud();
            }
        }

        public void Update(Point position, ToolInputState inputState)
        {
            if (!IsActive)
                return;

            Point constrained = _constraints.Apply(position, inputState);
            Vector deltaPretendido = constrained - _start;
            Vector incrementoPretendido = deltaPretendido - _deltaAplicado;
            Vector incremento = _alignmentGuides.AplicarSnap(_elementos, incrementoPretendido);

            if (Math.Abs(incremento.X) < 0.000001 && Math.Abs(incremento.Y) < 0.000001)
            {
                AtualizarHud();
                return;
            }

            foreach (ElementoViewModel vm in _elementos)
                _move.MoverVisual(vm, incremento);

            _deltaAplicado += incremento;
            AtualizarHud();
        }

        public void End()
        {
            if (!IsActive)
                return;

            _move.EndMove(_elementos);
            _constraints.End();
            _alignmentGuides.Limpar();
            OcultarHud();
            _elementos.Clear();
            _deltaAplicado = default;
            IsActive = false;
        }

        public void Cancel()
        {
            if (!IsActive)
                return;

            _move.AbortMove();
            _constraints.Cancel();
            _alignmentGuides.Limpar();
            OcultarHud();
            _elementos.Clear();
            _deltaAplicado = default;
            IsActive = false;
        }

        private void AtualizarHud()
        {
            if (!_mostrarHud || _elementos.Count == 0)
                return;

            _moveHud.DeltaX = _deltaAplicado.X;
            _moveHud.DeltaY = _deltaAplicado.Y;
            _moveHud.AtualizarPosicao(CalcularBounds(_elementos));
        }

        private void OcultarHud()
        {
            if (!_mostrarHud)
                return;

            _moveHud.Reset();
            _moveHud.Visivel = false;
        }

        private static Rect CalcularBounds(IReadOnlyList<ElementoViewModel> elementos)
        {
            Rect total = elementos[0].Bounds;

            for (int i = 1; i < elementos.Count; i++)
                total.Union(elementos[i].Bounds);

            return total;
        }
    }
}