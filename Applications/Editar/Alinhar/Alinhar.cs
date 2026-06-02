using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Core.SceneQueries;
using Araci.Services;
using Araci.ViewModels;
using Araci.Services.Editing;
using Araci.Services.Interaction;

namespace Araci.Applications.Editar.Alinhar
{
    public class AlinharTool : ITool
    {
        private readonly HoverService _hoverService;
        private readonly AlignmentGuideService _alignmentGuides;
        private readonly ICommandHistory _commands;
        private readonly ISceneQueryService _sceneQueries;
        private AlignReference? _reference;
        private AlignReference? _preview;
        private ElementoViewModel? _hover;

        public AlinharTool(
            HoverService hoverService,
            AlignmentGuideService alignmentGuides,
            ICommandHistory commands,
            ISceneQueryService sceneQueries)
        {
            _hoverService = hoverService ?? throw new ArgumentNullException(nameof(hoverService));
            _alignmentGuides = alignmentGuides ?? throw new ArgumentNullException(nameof(alignmentGuides));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
        }

        public string Nome => "Alinhar";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _reference != null || _preview != null;

        public void Ativar()
        {
            _hoverService.Clear();
            _alignmentGuides.Limpar();
        }

        public void Desativar()
        {
            Cancelar();
        }

        public void Cancelar()
        {
            if (_reference?.Elemento != null)
                _reference.Elemento.IsHover = false;

            LimparHover();
            _reference = null;
            _preview = null;
            _alignmentGuides.Limpar();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            AlignReference? clicked = CriarReferenciaDoMouse(vm, position);

            if (clicked == null)
                return;

            if (_reference == null)
            {
                DefinirReferencia(clicked);
                return;
            }

            if (ReferenceEquals(clicked.Elemento, _reference.Elemento))
            {
                DefinirReferencia(clicked);
                return;
            }

            AplicarAlinhamento(_reference, clicked);
            Cancelar();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            AlignReference? current = CriarReferenciaDoMouse(null, position);
            _preview = current;

            if (current == null)
            {
                LimparHover();

                if (_reference != null)
                    MostrarReferencia(_reference);
                else
                    _alignmentGuides.Limpar();

                return;
            }

            AtualizarHover(current.Elemento);

            if (_reference == null)
            {
                MostrarReferencia(current);
                return;
            }

            if (ReferenceEquals(current.Elemento, _reference.Elemento))
            {
                MostrarReferencia(current);
                return;
            }

            MostrarPreviewAlinhamento(_reference, current);
        }

        public void OnMouseUp(Point position, ToolInputState inputState)
        {
        }

        public bool HandlesKey(Key key)
        {
            return key == Key.Escape;
        }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Escape)
                Cancelar();
        }

        private void DefinirReferencia(AlignReference reference)
        {
            if (_reference?.Elemento != null && !ReferenceEquals(_reference.Elemento, reference.Elemento))
                _reference.Elemento.IsHover = false;

            _reference = reference;
            _reference.Elemento.IsHover = true;
            _preview = reference;
            MostrarReferencia(reference);
        }

        private AlignReference? CriarReferenciaDoMouse(ElementoViewModel? vm, Point position)
        {
            ElementoViewModel? elemento = vm ?? _sceneQueries.HitTest(position)?.Elemento;

            if (elemento == null || elemento.IsPreview || elemento.BoundsAlinhamento.IsEmpty)
                return null;

            return new AlignReference(elemento, DetectarAncora(elemento, position));
        }

        private void AplicarAlinhamento(AlignReference reference, AlignReference target)
        {
            Vector delta = CalcularDelta(reference, target);

            if (Math.Abs(delta.X) < 0.0001 && Math.Abs(delta.Y) < 0.0001)
                return;

            ElementoEstado antes = target.Elemento.CapturarEstado();
            target.Elemento.Mover(delta);
            ElementoEstado depois = target.Elemento.CapturarEstado();
            _commands.Execute(new AlignElementCommand(target.Elemento, antes, depois));
            _sceneQueries.Invalidate();
        }

        private void MostrarReferencia(AlignReference reference)
        {
            Rect bounds = reference.Elemento.BoundsAlinhamento;
            double value = reference.Anchor.GetCoordinate(bounds);

            if (reference.Anchor.Axis == AlignAxis.Vertical)
                _alignmentGuides.MostrarReferenciaVertical(value, bounds);
            else
                _alignmentGuides.MostrarReferenciaHorizontal(value, bounds);
        }

        private void MostrarPreviewAlinhamento(AlignReference reference, AlignReference target)
        {
            Vector delta = CalcularDelta(reference, target);
            Rect referenceBounds = reference.Elemento.BoundsAlinhamento;
            Rect targetBounds = target.Elemento.BoundsAlinhamento;
            Rect targetFinalBounds = Deslocar(targetBounds, delta);
            double referenceValue = reference.Anchor.GetCoordinate(referenceBounds);
            double targetValue = target.Anchor.GetCoordinate(targetBounds);

            if (reference.Anchor.Axis == AlignAxis.Vertical)
                _alignmentGuides.MostrarDuasReferenciasVerticais(referenceValue, referenceBounds, targetValue, targetBounds, targetFinalBounds);
            else
                _alignmentGuides.MostrarDuasReferenciasHorizontais(referenceValue, referenceBounds, targetValue, targetBounds, targetFinalBounds);
        }

        private void AtualizarHover(ElementoViewModel elemento)
        {
            if (ReferenceEquals(_hover, elemento))
                return;

            LimparHover();
            _hover = elemento;
            _hover.IsHover = true;
        }

        private void LimparHover()
        {
            if (_hover == null)
                return;

            if (!ReferenceEquals(_hover, _reference?.Elemento))
                _hover.IsHover = false;

            _hover = null;
        }

        private static Vector CalcularDelta(AlignReference reference, AlignReference target)
        {
            Rect referenceBounds = reference.Elemento.BoundsAlinhamento;
            Rect targetBounds = target.Elemento.BoundsAlinhamento;
            double referenceValue = reference.Anchor.GetCoordinate(referenceBounds);
            double targetValue = target.Anchor.GetCoordinate(targetBounds);
            double offset = referenceValue - targetValue;
            return reference.Anchor.Axis == AlignAxis.Vertical ? new Vector(offset, 0) : new Vector(0, offset);
        }

        private static AlignAnchor DetectarAncora(ElementoViewModel elemento, Point position)
        {
            Rect bounds = elemento.BoundsAlinhamento;

            if (elemento is BarraViewModel)
                return DetectarAncoraBarra(bounds, position);

            return DetectarAncoraGeral(bounds, position);
        }

        private static AlignAnchor DetectarAncoraBarra(Rect bounds, Point position)
        {
            double topDistance = Math.Abs(bounds.Top - position.Y);
            double centerDistance = Math.Abs((bounds.Top + bounds.Height / 2.0) - position.Y);
            double bottomDistance = Math.Abs(bounds.Bottom - position.Y);

            if (topDistance <= centerDistance && topDistance <= bottomDistance)
                return AlignAnchor.Top;

            if (bottomDistance <= centerDistance && bottomDistance <= topDistance)
                return AlignAnchor.Bottom;

            return AlignAnchor.CenterY;
        }

        private static AlignAnchor DetectarAncoraGeral(Rect bounds, Point position)
        {
            AlignAnchor[] anchors =
            {
                AlignAnchor.Left,
                AlignAnchor.Right,
                AlignAnchor.CenterX,
                AlignAnchor.Top,
                AlignAnchor.Bottom,
                AlignAnchor.CenterY
            };

            AlignAnchor best = AlignAnchor.Left;
            double bestDistance = double.MaxValue;

            foreach (AlignAnchor anchor in anchors)
            {
                double mouseValue = anchor.Axis == AlignAxis.Vertical ? position.X : position.Y;
                double distance = Math.Abs(anchor.GetCoordinate(bounds) - mouseValue);

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = anchor;
            }

            return best;
        }

        private static Rect Deslocar(Rect rect, Vector delta)
        {
            return new Rect(rect.X + delta.X, rect.Y + delta.Y, rect.Width, rect.Height);
        }

        private sealed class AlignReference
        {
            public AlignReference(ElementoViewModel elemento, AlignAnchor anchor)
            {
                Elemento = elemento;
                Anchor = anchor;
            }

            public ElementoViewModel Elemento { get; }
            public AlignAnchor Anchor { get; }
        }

        private readonly struct AlignAnchor
        {
            private readonly AlignAnchorKind _kind;

            private AlignAnchor(AlignAnchorKind kind, AlignAxis axis)
            {
                _kind = kind;
                Axis = axis;
            }

            public AlignAxis Axis { get; }
            public static AlignAnchor Left => new(AlignAnchorKind.Left, AlignAxis.Vertical);
            public static AlignAnchor Right => new(AlignAnchorKind.Right, AlignAxis.Vertical);
            public static AlignAnchor CenterX => new(AlignAnchorKind.CenterX, AlignAxis.Vertical);
            public static AlignAnchor Top => new(AlignAnchorKind.Top, AlignAxis.Horizontal);
            public static AlignAnchor Bottom => new(AlignAnchorKind.Bottom, AlignAxis.Horizontal);
            public static AlignAnchor CenterY => new(AlignAnchorKind.CenterY, AlignAxis.Horizontal);

            public double GetCoordinate(Rect bounds)
            {
                return _kind switch
                {
                    AlignAnchorKind.Left => bounds.Left,
                    AlignAnchorKind.Right => bounds.Right,
                    AlignAnchorKind.CenterX => bounds.Left + bounds.Width / 2.0,
                    AlignAnchorKind.Top => bounds.Top,
                    AlignAnchorKind.Bottom => bounds.Bottom,
                    AlignAnchorKind.CenterY => bounds.Top + bounds.Height / 2.0,
                    _ => 0
                };
            }
        }

        private enum AlignAnchorKind
        {
            Left,
            Right,
            CenterX,
            Top,
            Bottom,
            CenterY
        }

        private enum AlignAxis
        {
            Vertical,
            Horizontal
        }
    }
}
