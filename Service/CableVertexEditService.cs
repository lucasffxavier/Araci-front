using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class CableVertexEditService
    {
        private const double HandleTolerance = 8.0;

        private readonly EditorContext _context;

        private CaboViewModel? _caboAtivo;
        private int _indiceAtivo = -1;
        private ElementoEstado? _estadoInicial;
        private CaboViewModel? _handleAtivoCabo;
        private int _handleAtivoIndice = -1;
        private Point _pontoInicialArrasto;
        private OrthogonalAxis? _eixoOrtogonal;

        public CableVertexEditService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ObservableCollection<CableVertexHandleViewModel> Handles { get; } = new();

        public bool IsEditing => _caboAtivo != null;

        public void Refresh()
        {
            if (IsEditing)
                return;

            RebuildHandles();
        }

        public bool TryBegin(Point position)
        {
            var handle = HitTest(position);

            if (handle == null)
                return false;

            _caboAtivo = handle.Cabo;
            _indiceAtivo = handle.Indice;
            _estadoInicial = _caboAtivo.CapturarEstado();
            _pontoInicialArrasto = new Point(handle.X, handle.Y);
            _eixoOrtogonal = null;
            DefinirHandleAtivo(handle.Cabo, handle.Indice);

            return true;
        }

        public bool TryInsertVertex(Point position)
        {
            var hit = HitTestSegment(position);

            if (hit == null)
                return false;

            var cabo = hit.Cabo;
            var antes = cabo.CapturarEstado();

            cabo.Cabo.Vertices.Insert(hit.InsertIndex, hit.Point);
            cabo.AtualizarAposModeloAlterado();
            DefinirHandleAtivo(cabo, hit.InsertIndex);

            ExecutarAlteracao(cabo, antes);
            return true;
        }

        public bool TryRemoveHandle(Point position)
        {
            var handle = HitTest(position);

            if (handle == null)
                return false;

            return RemoveHandle(handle.Cabo, handle.Indice);
        }

        public bool TryRemoveActive()
        {
            if (_handleAtivoCabo == null)
                return false;

            return RemoveHandle(_handleAtivoCabo, _handleAtivoIndice);
        }

        public void Update(Point position, ToolInputState inputState)
        {
            if (_caboAtivo == null || !IndiceIntermediarioValido(_caboAtivo, _indiceAtivo))
                return;

            Point pontoEfetivo = AplicarRestricaoOrtogonal(position, inputState);

            _caboAtivo.Cabo.Vertices[_indiceAtivo] = pontoEfetivo;
            _caboAtivo.AtualizarAposModeloAlterado();
            _context.SceneQueries.Invalidate();

            RebuildHandles();
        }

        public void End()
        {
            if (_caboAtivo == null)
                return;

            var cabo = _caboAtivo;
            var antes = _estadoInicial;
            var depois = cabo.CapturarEstado();

            LimparEdicao();

            if (antes != null && !VerticesIguais(antes, depois))
                ExecutarAlteracao(cabo, antes);

            RebuildHandles();
        }

        public void Cancel()
        {
            if (_caboAtivo != null && _estadoInicial != null)
                _caboAtivo.AplicarEstado(_estadoInicial);

            LimparEdicao();
            _context.SceneQueries.Invalidate();
            RebuildHandles();
        }

        public void Clear()
        {
            LimparEdicao();
            LimparHandleAtivo();
            Handles.Clear();
        }

        private CableVertexHandleViewModel? HitTest(Point position)
        {
            double tolerance2 = HandleTolerance * HandleTolerance;

            return Handles
                .Where(h => DistanciaQuadrada(new Point(h.X, h.Y), position) <= tolerance2)
                .OrderBy(h => DistanciaQuadrada(new Point(h.X, h.Y), position))
                .FirstOrDefault();
        }

        private void RebuildHandles()
        {
            Handles.Clear();

            foreach (var cabo in _context.Selection.Selecionados.OfType<CaboViewModel>())
            {
                if (cabo.IsPreview || cabo.Cabo.Vertices.Count < 3)
                    continue;

                for (int i = 1; i < cabo.Cabo.Vertices.Count - 1; i++)
                {
                    Point p = cabo.Cabo.Vertices[i];
                    bool isActive = ReferenceEquals(cabo, _handleAtivoCabo) && i == _handleAtivoIndice;
                    Handles.Add(new CableVertexHandleViewModel(cabo, i, p.X, p.Y, isActive));
                }
            }
        }

        private SegmentHit? HitTestSegment(Point position)
        {
            double tolerance2 = HandleTolerance * HandleTolerance;
            SegmentHit? melhor = null;

            foreach (var cabo in _context.Selection.Selecionados.OfType<CaboViewModel>())
            {
                var vertices = cabo.Cabo.Vertices;

                if (cabo.IsPreview || vertices.Count < 2)
                    continue;

                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    Point projection = ProjetarNoSegmento(position, vertices[i], vertices[i + 1]);
                    double distance2 = DistanciaQuadrada(position, projection);

                    if (distance2 > tolerance2)
                        continue;

                    if (melhor == null || distance2 < melhor.DistanceSquared)
                        melhor = new SegmentHit(cabo, i + 1, projection, distance2);
                }
            }

            return melhor;
        }

        private bool RemoveHandle(CaboViewModel cabo, int indice)
        {
            if (!IndiceIntermediarioValido(cabo, indice))
                return false;

            var antes = cabo.CapturarEstado();

            cabo.Cabo.Vertices.RemoveAt(indice);
            cabo.AtualizarAposModeloAlterado();
            AjustarHandleAtivoAposRemocao(cabo, indice);

            ExecutarAlteracao(cabo, antes);
            return true;
        }

        private void ExecutarAlteracao(CaboViewModel cabo, ElementoEstado antes)
        {
            var depois = cabo.CapturarEstado();

            if (VerticesIguais(antes, depois))
            {
                RebuildHandles();
                return;
            }

            _context.Commands.Execute(
                new MoveElementoCommand(
                    cabo.Modelo,
                    antes,
                    depois,
                    AtualizarCabo));

            _context.SceneQueries.Invalidate();
            RebuildHandles();
        }

        private void AtualizarCabo(Elemento elemento)
        {
            _context.Viewport?.AtualizarViewModel(elemento);
            _context.SceneQueries.Invalidate();
            RebuildHandles();
        }

        private void LimparEdicao()
        {
            _caboAtivo = null;
            _indiceAtivo = -1;
            _estadoInicial = null;
            _eixoOrtogonal = null;
        }

        private void DefinirHandleAtivo(CaboViewModel cabo, int indice)
        {
            _handleAtivoCabo = cabo;
            _handleAtivoIndice = indice;
            RebuildHandles();
        }

        private void LimparHandleAtivo()
        {
            _handleAtivoCabo = null;
            _handleAtivoIndice = -1;
        }

        private void AjustarHandleAtivoAposRemocao(CaboViewModel cabo, int indiceRemovido)
        {
            if (!ReferenceEquals(cabo, _handleAtivoCabo))
                return;

            if (cabo.Cabo.Vertices.Count < 3)
            {
                LimparHandleAtivo();
                return;
            }

            int novoIndice = Math.Min(indiceRemovido, cabo.Cabo.Vertices.Count - 2);
            DefinirHandleAtivo(cabo, novoIndice);
        }

        private static bool IndiceIntermediarioValido(CaboViewModel cabo, int indice)
        {
            return indice > 0 && indice < cabo.Cabo.Vertices.Count - 1;
        }

        private static bool VerticesIguais(ElementoEstado a, ElementoEstado b)
        {
            return a.Vertices.SequenceEqual(b.Vertices);
        }

        private static Point ProjetarNoSegmento(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;

            if (lengthSquared <= double.Epsilon)
                return a;

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));

            return new Point(
                a.X + t * dx,
                a.Y + t * dy);
        }

        private static double DistanciaQuadrada(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;

            return dx * dx + dy * dy;
        }

        private Point AplicarRestricaoOrtogonal(Point position, ToolInputState inputState)
        {
            if (!inputState.IsShiftPressed)
            {
                _eixoOrtogonal = null;
                return position;
            }

            Vector total = position - _pontoInicialArrasto;

            if (!_eixoOrtogonal.HasValue)
            {
                if (Math.Abs(total.X) < 0.0001 && Math.Abs(total.Y) < 0.0001)
                    return _pontoInicialArrasto;

                _eixoOrtogonal = Math.Abs(total.X) >= Math.Abs(total.Y)
                    ? OrthogonalAxis.Horizontal
                    : OrthogonalAxis.Vertical;
            }

            return _eixoOrtogonal == OrthogonalAxis.Horizontal
                ? new Point(position.X, _pontoInicialArrasto.Y)
                : new Point(_pontoInicialArrasto.X, position.Y);
        }

        private enum OrthogonalAxis
        {
            Horizontal,
            Vertical
        }
    }

    public class CableVertexHandleViewModel
    {
        public CableVertexHandleViewModel(CaboViewModel cabo, int indice, double x, double y, bool isActive)
        {
            Cabo = cabo;
            Indice = indice;
            X = x;
            Y = y;
            IsActive = isActive;
        }

        public CaboViewModel Cabo { get; }
        public int Indice { get; }
        public double X { get; }
        public double Y { get; }
        public bool IsActive { get; }
    }

    internal sealed class SegmentHit
    {
        public SegmentHit(CaboViewModel cabo, int insertIndex, Point point, double distanceSquared)
        {
            Cabo = cabo;
            InsertIndex = insertIndex;
            Point = point;
            DistanceSquared = distanceSquared;
        }

        public CaboViewModel Cabo { get; }
        public int InsertIndex { get; }
        public Point Point { get; }
        public double DistanceSquared { get; }
    }
}
