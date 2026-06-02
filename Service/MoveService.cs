using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.UseCases.Editar;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;
using Araci.Services.Geometry;
using Araci.Services.Topology;

namespace Araci.Services
{
    public class MoveService
    {
        private readonly ConnectivityService _connectivity;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly Func<ViewportService?> _viewportProvider;
        private readonly ISceneQueryService _sceneQueries;
        private readonly MoverElementoUseCase _moverElemento;

        private readonly Dictionary<ElementoViewModel, ElementoEstado>
            _estadoInicial = new();

        private bool _movendo;

        public MoveService(
            ConnectivityService connectivity,
            TerminalLayoutService terminalLayout,
            Func<ViewportService?> viewportProvider,
            ISceneQueryService sceneQueries,
            MoverElementoUseCase moverElemento)
        {
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _viewportProvider = viewportProvider ?? throw new ArgumentNullException(nameof(viewportProvider));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _moverElemento = moverElemento ?? throw new ArgumentNullException(nameof(moverElemento));
        }

        public void BeginMove(IEnumerable<ElementoViewModel> elementos)
        {
            _estadoInicial.Clear();

            foreach (var vm in elementos.Distinct())
                CapturarEstadoInicial(vm);

            _movendo = true;
        }

        public void MoverVisual(
            ElementoViewModel vm,
            Vector delta)
        {
            if (!_movendo)
                return;

            vm.Mover(delta);
            ReancorarCabosConectados(vm.Modelo);
            _sceneQueries.Invalidate();
        }

        public void EndMove(IEnumerable<ElementoViewModel> elementos)
        {
            if (!_movendo)
                return;

            foreach (var vm in elementos.Distinct())
                CapturarCabosConectados(vm.Modelo);

            var items = new List<MoverElementoItem>();

            foreach (var item in _estadoInicial)
            {
                var vm = item.Key;
                var antes = item.Value;

                var depois = vm.CapturarEstado();
                items.Add(new MoverElementoItem(vm.Modelo, antes, depois));
            }

            _moverElemento.Executar(items);
            _sceneQueries.Invalidate();

            _estadoInicial.Clear();

            _movendo = false;
        }

        public void AbortMove()
        {
            if (!_movendo)
                return;

            foreach (var item in _estadoInicial)
                item.Key.AplicarEstado(item.Value);

            _sceneQueries.Invalidate();

            _estadoInicial.Clear();
            _movendo = false;
        }

        private void CapturarEstadoInicial(ElementoViewModel vm)
        {
            if (_estadoInicial.ContainsKey(vm))
                return;

            _estadoInicial[vm] = vm.CapturarEstado();
            CapturarCabosConectados(vm.Modelo);
        }

        private void CapturarCabosConectados(Elemento elemento)
        {
            if (elemento is Cabo)
                return;

            ViewportService? viewport = _viewportProvider();

            foreach (Cabo cabo in _connectivity.ObterCabosConectados(elemento))
            {
                ElementoViewModel? caboVm = viewport?.ObterViewModel(cabo);

                if (caboVm != null && !_estadoInicial.ContainsKey(caboVm))
                    _estadoInicial[caboVm] = caboVm.CapturarEstado();
            }
        }

        private void ReancorarCabosConectados(Elemento elemento)
        {
            if (elemento is Cabo)
                return;

            _terminalLayout.AtualizarTerminais(elemento);

            ViewportService? viewport = _viewportProvider();

            foreach (Cabo cabo in _connectivity.ReancorarCabosConectados(elemento))
                viewport?.AtualizarViewModel(cabo);
        }
    }
}
