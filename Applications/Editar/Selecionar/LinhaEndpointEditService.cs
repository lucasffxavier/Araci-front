using System;
using System.Collections.Generic;
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
    public class LinhaEndpointEditService
    {
        private const double ToleranciaSnapInsercao = 12.0;
        private readonly ISelectionService _selection;
        private readonly ISceneQueryService _sceneQueries;
        private readonly MoverElementoUseCase _moverElemento;
        private readonly Action<Elemento> _onStateApplied;
        private readonly LinhaEndpointInteractionController _interaction = new();

        private ElementoEstado? _estadoInicial;
        private LinhaAnotativaViewModel? _handleAtivoLinha;
        private LinhaEndpointKind? _handleAtivoKind;

        public LinhaEndpointEditService(
            ISelectionService selection,
            ISceneQueryService sceneQueries,
            MoverElementoUseCase moverElemento,
            Action<Elemento> onStateApplied)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _moverElemento = moverElemento ?? throw new ArgumentNullException(nameof(moverElemento));
            _onStateApplied = onStateApplied ?? throw new ArgumentNullException(nameof(onStateApplied));
        }

        public ObservableCollection<LinhaEndpointHandleViewModel> Handles { get; } = new();
        public ObservableCollection<LinhaEndpointSnapViewModel> InsertionSnapHandles { get; } = new();
        public bool IsEditing => _interaction.IsDragging;

        public void Refresh()
        {
            if (IsEditing)
                return;

            RebuildHandles();
        }

        public bool TryBegin(Point position)
        {
            LinhaEndpointHandleViewModel? handle = _interaction.HitTest(Handles, position);

            if (handle == null)
                return false;

            _interaction.BeginDrag(handle);
            _estadoInicial = handle.Linha.CapturarEstado();
            DefinirHandleAtivo(handle.Linha, handle.Kind);
            return true;
        }

        public void Update(Point position, ToolInputState inputState)
        {
            if (_interaction.LinhaAtiva == null || !_interaction.PontaAtiva.HasValue)
                return;

            Point pontoEfetivo = _interaction.AplicarRestricaoOrtogonal(position, inputState);
            AplicarPonta(_interaction.LinhaAtiva, _interaction.PontaAtiva.Value, pontoEfetivo);
            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void End()
        {
            if (_interaction.LinhaAtiva == null)
                return;

            LinhaAnotativaViewModel linha = _interaction.LinhaAtiva;
            ElementoEstado? antes = _estadoInicial;
            ElementoEstado depois = linha.CapturarEstado();

            LimparEdicao();

            if (antes != null)
                ExecutarAlteracao(linha, antes, depois);

            RebuildHandles();
        }

        public void Cancel()
        {
            if (_interaction.LinhaAtiva != null && _estadoInicial != null)
            {
                _interaction.LinhaAtiva.AplicarEstado(_estadoInicial);
                _onStateApplied(_interaction.LinhaAtiva.Modelo);
            }

            LimparEdicao();
            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        public void Clear()
        {
            LimparEdicao();
            LimparHandleAtivo();
            LimparSnapInsercao();
            Handles.Clear();
        }

        public Point? AtualizarSnapInsercao(IEnumerable<ElementoViewModel> elementos, Point position)
        {
            Point? snap = ObterPontaMaisProxima(elementos, position, ToleranciaSnapInsercao);
            DefinirSnapInsercao(snap);
            return snap;
        }

        public void LimparSnapInsercao()
        {
            if (InsertionSnapHandles.Count == 0)
                return;

            InsertionSnapHandles.Clear();
        }

        private void DefinirSnapInsercao(Point? ponto)
        {
            InsertionSnapHandles.Clear();

            if (ponto.HasValue)
                InsertionSnapHandles.Add(new LinhaEndpointSnapViewModel(ponto.Value.X, ponto.Value.Y));
        }

        private static Point? ObterPontaMaisProxima(IEnumerable<ElementoViewModel> elementos, Point position, double tolerancia)
        {
            double melhorDistancia = tolerancia * tolerancia;
            Point? melhor = null;

            foreach (LinhaAnotativaViewModel linha in elementos.OfType<LinhaAnotativaViewModel>())
            {
                if (linha.IsPreview)
                    continue;

                Avaliar(linha.PontoInicial);
                Avaliar(linha.PontoFinal);
            }

            return melhor;

            void Avaliar(Point ponto)
            {
                double dx = ponto.X - position.X;
                double dy = ponto.Y - position.Y;
                double distancia = dx * dx + dy * dy;

                if (distancia > melhorDistancia)
                    return;

                melhorDistancia = distancia;
                melhor = ponto;
            }
        }

        private void RebuildHandles()
        {
            Handles.Clear();

            foreach (LinhaAnotativaViewModel linha in _selection.Selecionados.OfType<LinhaAnotativaViewModel>())
            {
                if (linha.IsPreview)
                    continue;

                Handles.Add(new LinhaEndpointHandleViewModel(
                    linha,
                    LinhaEndpointKind.Inicio,
                    linha.PontoInicial.X,
                    linha.PontoInicial.Y,
                    ReferenceEquals(linha, _handleAtivoLinha) && _handleAtivoKind == LinhaEndpointKind.Inicio));

                Handles.Add(new LinhaEndpointHandleViewModel(
                    linha,
                    LinhaEndpointKind.Fim,
                    linha.PontoFinal.X,
                    linha.PontoFinal.Y,
                    ReferenceEquals(linha, _handleAtivoLinha) && _handleAtivoKind == LinhaEndpointKind.Fim));
            }
        }

        private static void AplicarPonta(LinhaAnotativaViewModel vm, LinhaEndpointKind kind, Point ponto)
        {
            if (kind == LinhaEndpointKind.Fim)
            {
                vm.X2 = ponto.X - vm.Linha.PosicaoX;
                vm.Y2 = ponto.Y - vm.Linha.PosicaoY;
                return;
            }

            Point fim = vm.PontoFinal;
            vm.X = ponto.X;
            vm.Y = ponto.Y;
            vm.X2 = fim.X - ponto.X;
            vm.Y2 = fim.Y - ponto.Y;
        }

        private void ExecutarAlteracao(LinhaAnotativaViewModel linha, ElementoEstado antes, ElementoEstado depois)
        {
            if (!Mudou(antes, depois))
            {
                RebuildHandles();
                return;
            }

            _moverElemento.Executar(new[]
            {
                new MoverElementoItem(linha.Modelo, antes, depois)
            });

            _sceneQueries.Invalidate();
            RebuildHandles();
        }

        private void LimparEdicao()
        {
            _interaction.ClearDrag();
            _estadoInicial = null;
        }

        private void DefinirHandleAtivo(LinhaAnotativaViewModel linha, LinhaEndpointKind kind)
        {
            _handleAtivoLinha = linha;
            _handleAtivoKind = kind;
            RebuildHandles();
        }

        private void LimparHandleAtivo()
        {
            _handleAtivoLinha = null;
            _handleAtivoKind = null;
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