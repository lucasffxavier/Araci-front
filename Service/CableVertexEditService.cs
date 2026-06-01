using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Araci.Applications.Editar.Base;
using Araci.Applications.UseCases.Editar;
using Araci.Applications.Editar.Selecionar;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class CableVertexEditService
    {
        private readonly SelectionService _selection;
        private readonly ISceneQueryService _sceneQueries;
        private readonly VisualUpdateService _visualUpdates;
        private readonly EditarVerticesCaboUseCase _editarVerticesCabo;
        private readonly CableVertexInteractionController _interaction = new();

        private ElementoEstado? _estadoInicial;
        private CaboViewModel? _handleAtivoCabo;
        private int _handleAtivoIndice = -1;

        public CableVertexEditService(
            SelectionService selection,
            ISceneQueryService sceneQueries,
            VisualUpdateService visualUpdates,
            EditarVerticesCaboUseCase editarVerticesCabo)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _visualUpdates = visualUpdates ?? throw new ArgumentNullException(nameof(visualUpdates));
            _editarVerticesCabo = editarVerticesCabo ?? throw new ArgumentNullException(nameof(editarVerticesCabo));
        }

        public ObservableCollection<CableVertexHandleViewModel> Handles { get; } = new();

        public bool IsEditing => _interaction.IsDragging;

        public void Refresh()
        {
            if (IsEditing)
                return;

            RebuildHandles();
        }

        public bool TryBegin(Point position)
        {
            var handle = _interaction.HitTest(Handles, position);

            if (handle == null)
                return false;

            _interaction.BeginDrag(handle);
            _estadoInicial = handle.Cabo.CapturarEstado();
            DefinirHandleAtivo(handle.Cabo, handle.Indice);

            return true;
        }

        public bool TryInsertVertex(Point position)
        {
            var hit = _interaction.HitTestSegment(_selection.Selecionados.OfType<CaboViewModel>(), position);

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
            var handle = _interaction.HitTest(Handles, position);

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
            CaboViewModel? caboAtivo = _interaction.CaboAtivo;
            int indiceAtivo = _interaction.IndiceAtivo;

            if (caboAtivo == null || !CableVertexInteractionController.IndiceIntermediarioValido(caboAtivo, indiceAtivo))
                return;

            Point pontoEfetivo = _interaction.AplicarRestricaoOrtogonal(position, inputState);

            caboAtivo.Cabo.Vertices[indiceAtivo] = pontoEfetivo;
            caboAtivo.AtualizarAposModeloAlterado();
            _sceneQueries.Invalidate();

            RebuildHandles();
        }

        public void End()
        {
            if (_interaction.CaboAtivo == null)
                return;

            var cabo = _interaction.CaboAtivo;
            var antes = _estadoInicial;
            var depois = cabo.CapturarEstado();

            LimparEdicao();

            if (antes != null && !VerticesIguais(antes, depois))
                ExecutarAlteracao(cabo, antes);

            RebuildHandles();
        }

        public void Cancel()
        {
            if (_interaction.CaboAtivo != null && _estadoInicial != null)
                _interaction.CaboAtivo.AplicarEstado(_estadoInicial);

            LimparEdicao();
            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void Clear()
        {
            LimparEdicao();
            LimparHandleAtivo();
            Handles.Clear();
        }

        private void RebuildHandles()
        {
            Handles.Clear();

            foreach (var cabo in _selection.Selecionados.OfType<CaboViewModel>())
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

        private bool RemoveHandle(CaboViewModel cabo, int indice)
        {
            if (!CableVertexInteractionController.IndiceIntermediarioValido(cabo, indice))
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

            var request = new EditarVerticesCaboRequest(cabo.Cabo, antes, depois);
            _editarVerticesCabo.Executar(request, AtualizarCabo);

            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        private void AtualizarCabo(Elemento elemento)
        {
            _visualUpdates.AtualizarCaboEditado(elemento);
            RebuildHandles();
        }

        private void LimparEdicao()
        {
            _interaction.ClearDrag();
            _estadoInicial = null;
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

        private static bool VerticesIguais(ElementoEstado a, ElementoEstado b)
        {
            return a.Vertices.SequenceEqual(b.Vertices);
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

}
