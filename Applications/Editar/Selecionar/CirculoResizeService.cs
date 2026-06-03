using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Applications.UseCases.Editar;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class CirculoResizeService
    {
        private const double HitTolerance = 10.0;
        private const double MinRadius = 1.0;

        private readonly ISelectionService _selection;
        private readonly ISceneQueryService _sceneQueries;
        private readonly MoverElementoUseCase _moverElemento;
        private readonly Action<Elemento> _onStateApplied;
        private CirculoAnotativoViewModel? _circuloAtivo;
        private ElementoEstado? _estadoInicial;

        public CirculoResizeService(ISelectionService selection, ISceneQueryService sceneQueries, MoverElementoUseCase moverElemento, Action<Elemento> onStateApplied)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _moverElemento = moverElemento ?? throw new ArgumentNullException(nameof(moverElemento));
            _onStateApplied = onStateApplied ?? throw new ArgumentNullException(nameof(onStateApplied));
        }

        public ObservableCollection<CirculoResizeHandleViewModel> Handles { get; } = new();
        public bool IsResizing => _circuloAtivo != null;

        public void Refresh()
        {
            if (IsResizing)
                return;

            RebuildHandles();
        }

        public bool TryBegin(Point position)
        {
            CirculoResizeHandleViewModel? handle = HitTest(position);

            if (handle == null)
                return false;

            _circuloAtivo = handle.Circulo;
            _estadoInicial = handle.Circulo.CapturarEstado();
            RebuildHandles();
            return true;
        }

        public void Update(Point position)
        {
            if (_circuloAtivo == null)
                return;

            Point centro = new(_circuloAtivo.Circulo.PosicaoX, _circuloAtivo.Circulo.PosicaoY);
            _circuloAtivo.Raio = Math.Max(MinRadius, Distancia(centro, position));
            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void End()
        {
            if (_circuloAtivo == null)
                return;

            CirculoAnotativoViewModel circulo = _circuloAtivo;
            ElementoEstado? antes = _estadoInicial;
            ElementoEstado depois = circulo.CapturarEstado();

            ClearResize();

            if (antes != null && Mudou(antes, depois))
            {
                _moverElemento.Executar(new[]
                {
                    new MoverElementoItem(circulo.Modelo, antes, depois)
                });
            }

            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void Cancel()
        {
            if (_circuloAtivo != null && _estadoInicial != null)
            {
                _circuloAtivo.AplicarEstado(_estadoInicial);
                _onStateApplied(_circuloAtivo.Modelo);
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

        private CirculoResizeHandleViewModel? HitTest(Point position)
        {
            double melhor = HitTolerance * HitTolerance;
            CirculoResizeHandleViewModel? encontrado = null;

            foreach (CirculoResizeHandleViewModel handle in Handles)
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

            foreach (CirculoAnotativoViewModel circulo in _selection.Selecionados.OfType<CirculoAnotativoViewModel>())
            {
                if (circulo.IsPreview)
                    continue;

                double x = circulo.Circulo.PosicaoX + circulo.Circulo.Raio;
                double y = circulo.Circulo.PosicaoY;
                Handles.Add(new CirculoResizeHandleViewModel(circulo, x, y, ReferenceEquals(circulo, _circuloAtivo)));
            }
        }

        private void ClearResize()
        {
            _circuloAtivo = null;
            _estadoInicial = null;
        }

        private static double Distancia(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
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