using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Base;
using Araci.Applications.UseCases.Editar;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class RetanguloResizeService
    {
        private const double HitTolerance = 10.0;
        private const double MinSize = 1.0;

        private readonly ISelectionService _selection;
        private readonly ISceneQueryService _sceneQueries;
        private readonly MoverElementoUseCase _moverElemento;
        private readonly Action<Elemento> _onStateApplied;

        private RetanguloAnotativoViewModel? _retanguloAtivo;
        private RetanguloResizeHandleKind? _handleAtivo;
        private ElementoEstado? _estadoInicial;
        private Rect _boundsInicial;

        public RetanguloResizeService(ISelectionService selection, ISceneQueryService sceneQueries, MoverElementoUseCase moverElemento, Action<Elemento> onStateApplied)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _moverElemento = moverElemento ?? throw new ArgumentNullException(nameof(moverElemento));
            _onStateApplied = onStateApplied ?? throw new ArgumentNullException(nameof(onStateApplied));
        }

        public ObservableCollection<RetanguloResizeHandleViewModel> Handles { get; } = new();
        public bool IsResizing => _retanguloAtivo != null && _handleAtivo.HasValue;

        public void Refresh()
        {
            if (IsResizing)
                return;

            RebuildHandles();
        }

        public bool TryBegin(Point position)
        {
            RetanguloResizeHandleViewModel? handle = HitTest(position);

            if (handle == null)
                return false;

            _retanguloAtivo = handle.Retangulo;
            _handleAtivo = handle.Kind;
            _estadoInicial = handle.Retangulo.CapturarEstado();
            _boundsInicial = handle.Retangulo.Bounds;
            RebuildHandles();
            return true;
        }

        public void Update(Point position, ToolInputState inputState)
        {
            if (_retanguloAtivo == null || !_handleAtivo.HasValue)
                return;

            Rect novoBounds = CalcularNovoBounds(_boundsInicial, _handleAtivo.Value, position, inputState.IsShiftPressed);
            AplicarBounds(_retanguloAtivo, novoBounds);
            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void End()
        {
            if (_retanguloAtivo == null)
                return;

            RetanguloAnotativoViewModel retangulo = _retanguloAtivo;
            ElementoEstado? antes = _estadoInicial;
            ElementoEstado depois = retangulo.CapturarEstado();

            ClearResize();

            if (antes != null && Mudou(antes, depois))
            {
                _moverElemento.Executar(new[]
                {
                    new MoverElementoItem(retangulo.Modelo, antes, depois)
                });
            }

            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void Cancel()
        {
            if (_retanguloAtivo != null && _estadoInicial != null)
            {
                _retanguloAtivo.AplicarEstado(_estadoInicial);
                _onStateApplied(_retanguloAtivo.Modelo);
            }

            ClearResize();
            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void Clear()
        {
            ClearResize();
            Handles.Clear();
        }

        private RetanguloResizeHandleViewModel? HitTest(Point position)
        {
            double melhor = HitTolerance * HitTolerance;
            RetanguloResizeHandleViewModel? encontrado = null;

            foreach (RetanguloResizeHandleViewModel handle in Handles)
            {
                double dx = handle.X - position.X;
                double dy = handle.Y - position.Y;
                double d = dx * dx + dy * dy;

                if (d > melhor)
                    continue;

                melhor = d;
                encontrado = handle;
            }

            return encontrado;
        }

        private void RebuildHandles()
        {
            Handles.Clear();

            foreach (RetanguloAnotativoViewModel retangulo in _selection.Selecionados.OfType<RetanguloAnotativoViewModel>())
            {
                if (retangulo.IsPreview)
                    continue;

                Rect b = retangulo.Bounds;
                Add(retangulo, RetanguloResizeHandleKind.TopLeft, b.Left, b.Top);
                Add(retangulo, RetanguloResizeHandleKind.Top, b.Left + b.Width / 2, b.Top);
                Add(retangulo, RetanguloResizeHandleKind.TopRight, b.Right, b.Top);
                Add(retangulo, RetanguloResizeHandleKind.Right, b.Right, b.Top + b.Height / 2);
                Add(retangulo, RetanguloResizeHandleKind.BottomRight, b.Right, b.Bottom);
                Add(retangulo, RetanguloResizeHandleKind.Bottom, b.Left + b.Width / 2, b.Bottom);
                Add(retangulo, RetanguloResizeHandleKind.BottomLeft, b.Left, b.Bottom);
                Add(retangulo, RetanguloResizeHandleKind.Left, b.Left, b.Top + b.Height / 2);
            }
        }

        private void Add(RetanguloAnotativoViewModel retangulo, RetanguloResizeHandleKind kind, double x, double y)
        {
            Handles.Add(new RetanguloResizeHandleViewModel(
                retangulo,
                kind,
                x,
                y,
                ReferenceEquals(retangulo, _retanguloAtivo) && _handleAtivo == kind));
        }

        private static Rect CalcularNovoBounds(Rect inicial, RetanguloResizeHandleKind kind, Point p, bool manterQuadrado)
        {
            double left = inicial.Left;
            double top = inicial.Top;
            double right = inicial.Right;
            double bottom = inicial.Bottom;

            if (kind is RetanguloResizeHandleKind.TopLeft or RetanguloResizeHandleKind.Left or RetanguloResizeHandleKind.BottomLeft)
                left = Math.Min(p.X, right - MinSize);

            if (kind is RetanguloResizeHandleKind.TopRight or RetanguloResizeHandleKind.Right or RetanguloResizeHandleKind.BottomRight)
                right = Math.Max(p.X, left + MinSize);

            if (kind is RetanguloResizeHandleKind.TopLeft or RetanguloResizeHandleKind.Top or RetanguloResizeHandleKind.TopRight)
                top = Math.Min(p.Y, bottom - MinSize);

            if (kind is RetanguloResizeHandleKind.BottomLeft or RetanguloResizeHandleKind.Bottom or RetanguloResizeHandleKind.BottomRight)
                bottom = Math.Max(p.Y, top + MinSize);

            if (manterQuadrado)
                AplicarQuadrado(kind, ref left, ref top, ref right, ref bottom, inicial);

            return new Rect(new Point(left, top), new Point(right, bottom));
        }

        private static void AplicarQuadrado(RetanguloResizeHandleKind kind, ref double left, ref double top, ref double right, ref double bottom, Rect inicial)
        {
            double largura = Math.Max(MinSize, right - left);
            double altura = Math.Max(MinSize, bottom - top);
            double lado = Math.Max(largura, altura);

            switch (kind)
            {
                case RetanguloResizeHandleKind.TopLeft:
                    left = right - lado;
                    top = bottom - lado;
                    break;
                case RetanguloResizeHandleKind.TopRight:
                    right = left + lado;
                    top = bottom - lado;
                    break;
                case RetanguloResizeHandleKind.BottomRight:
                    right = left + lado;
                    bottom = top + lado;
                    break;
                case RetanguloResizeHandleKind.BottomLeft:
                    left = right - lado;
                    bottom = top + lado;
                    break;
                case RetanguloResizeHandleKind.Left:
                    left = right - largura;
                    bottom = top + largura;
                    break;
                case RetanguloResizeHandleKind.Right:
                    right = left + largura;
                    bottom = top + largura;
                    break;
                case RetanguloResizeHandleKind.Top:
                    top = bottom - altura;
                    right = left + altura;
                    break;
                case RetanguloResizeHandleKind.Bottom:
                    bottom = top + altura;
                    right = left + altura;
                    break;
            }
        }

        private static void AplicarBounds(RetanguloAnotativoViewModel vm, Rect bounds)
        {
            vm.X = bounds.X;
            vm.Y = bounds.Y;
            vm.LarguraRetangulo = bounds.Width;
            vm.AlturaRetangulo = bounds.Height;
        }

        private void ClearResize()
        {
            _retanguloAtivo = null;
            _handleAtivo = null;
            _estadoInicial = null;
            _boundsInicial = Rect.Empty;
        }

        private static bool Mudou(ElementoEstado antes, ElementoEstado depois)
        {
            return Math.Abs(antes.X - depois.X) > 0.000001 ||
                Math.Abs(antes.Y - depois.Y) > 0.000001 ||
                Math.Abs(antes.X2 - depois.X2) > 0.000001 ||
                Math.Abs(antes.Y2 - depois.Y2) > 0.000001 ||
                Math.Abs(antes.Rotacao - depois.Rotacao) > 0.000001;
        }
    }
}