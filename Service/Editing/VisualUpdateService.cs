using System;
using System.Collections.Generic;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;
using Araci.Services.Geometry;
using Araci.Services.Topology;
using Araci.Services;
using Araci.Services.Viewport;

namespace Araci.Services.Editing
{
    public class VisualUpdateService
    {
        private readonly Func<ViewportService?> _viewportProvider;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly ConnectivityService _connectivity;
        private readonly ISceneQueryService _sceneQueries;
        private readonly TerminalSnapState _terminalSnap;
        private readonly Action _refreshEditingHandles;

        public VisualUpdateService(
            Func<ViewportService?> viewportProvider,
            TerminalLayoutService terminalLayout,
            ConnectivityService connectivity,
            ISceneQueryService sceneQueries,
            TerminalSnapState terminalSnap,
            Action refreshEditingHandles)
        {
            _viewportProvider = viewportProvider ?? throw new ArgumentNullException(nameof(viewportProvider));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _terminalSnap = terminalSnap ?? throw new ArgumentNullException(nameof(terminalSnap));
            _refreshEditingHandles = refreshEditingHandles ?? throw new ArgumentNullException(nameof(refreshEditingHandles));
        }

        public void AtualizarElementoMovido(Elemento elemento)
        {
            _viewportProvider()?.AtualizarViewModel(elemento);
            _terminalLayout.AtualizarTerminais(elemento);
            _sceneQueries.Invalidate();
            _refreshEditingHandles();
        }

        public void AtualizarElementoRotacionado(Elemento elemento)
        {
            if (elemento is Cabo cabo)
            {
                AtualizarCabo(cabo);
                AtualizarEstadoRotacao();
                return;
            }

            _terminalLayout.AtualizarTerminais(elemento);

            IReadOnlyList<Cabo> cabosReancorados =
                _connectivity.ReancorarCabosConectados(elemento);

            _viewportProvider()?.AtualizarViewModel(elemento);

            foreach (Cabo caboReancorado in cabosReancorados)
                AtualizarCabo(caboReancorado);

            AtualizarEstadoRotacao();
        }

        public void AtualizarCaboEditado(Elemento elemento)
        {
            _viewportProvider()?.AtualizarViewModel(elemento);
            _sceneQueries.Invalidate();
        }

        public void AtualizarGeometriaElementoECabos(Elemento elemento, IEnumerable<Cabo> cabos)
        {
            ViewportService? viewport = _viewportProvider();
            viewport?.AtualizarViewModel(elemento);

            foreach (Cabo cabo in cabos)
                viewport?.AtualizarViewModel(cabo);

            _sceneQueries.Invalidate();
            _terminalSnap.Limpar();
            _refreshEditingHandles();
        }

        private void AtualizarCabo(Cabo cabo)
        {
            _terminalLayout.AtualizarTerminais(cabo);
            _viewportProvider()?.AtualizarViewModel(cabo);
        }

        private void AtualizarEstadoRotacao()
        {
            _sceneQueries.Invalidate();
            _terminalSnap.Limpar();
            _refreshEditingHandles();
        }
    }
}
