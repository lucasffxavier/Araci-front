using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

            return true;
        }

        public void Update(Point position)
        {
            if (_caboAtivo == null || !IndiceIntermediarioValido(_caboAtivo, _indiceAtivo))
                return;

            _caboAtivo.Cabo.Vertices[_indiceAtivo] = position;
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
            {
                _context.Commands.Execute(
                    new MoveElementoCommand(
                        cabo.Modelo,
                        antes,
                        depois,
                        AtualizarCabo));
            }

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
                    Handles.Add(new CableVertexHandleViewModel(cabo, i, p.X, p.Y));
                }
            }
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
        }

        private static bool IndiceIntermediarioValido(CaboViewModel cabo, int indice)
        {
            return indice > 0 && indice < cabo.Cabo.Vertices.Count - 1;
        }

        private static bool VerticesIguais(ElementoEstado a, ElementoEstado b)
        {
            return a.Vertices.SequenceEqual(b.Vertices);
        }

        private static double DistanciaQuadrada(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;

            return dx * dx + dy * dy;
        }
    }

    public class CableVertexHandleViewModel
    {
        public CableVertexHandleViewModel(CaboViewModel cabo, int indice, double x, double y)
        {
            Cabo = cabo;
            Indice = indice;
            X = x;
            Y = y;
        }

        public CaboViewModel Cabo { get; }
        public int Indice { get; }
        public double X { get; }
        public double Y { get; }
    }
}
