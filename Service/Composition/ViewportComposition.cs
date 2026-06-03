using System;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Selecionar;
using Araci.Applications.Factories;
using Araci.Applications.Scene;
using Araci.Core.Documents;
using Araci.Core.SceneQueries;
using Araci.ViewModels;
using CoreScene = Araci.Core.Scenes.Scene;
using Araci.Services.Editing;
using Araci.Services.Viewport;
using Araci.Services.Interaction;

namespace Araci.Services.Composition
{
    internal static class ViewportComposition
    {
        public static ViewportViewModel CreateViewModel(
            AraciDocument document,
            CoreScene scene,
            SelectionBoxViewModel selectionBox,
            TerminalSnapState terminalSnap,
            CableVertexEditService cableVertexEdit,
            LinhaEndpointEditService linhaEndpointEdit,
            RetanguloResizeService retanguloResize,
            CirculoResizeService circuloResize,
            MoveHudService moveHud,
            AlignmentGuideService alignmentGuides,
            ElementoFactory elementoFactory,
            ISelectionService selection,
            IHoverService hover,
            ISceneQueryService sceneQueries)
        {
            var documentSceneSync = new DocumentSceneSyncService(
                document,
                scene,
                elementoFactory,
                selection,
                cableVertexEdit,
                terminalSnap,
                alignmentGuides,
                hover,
                sceneQueries);

            return new ViewportViewModel(
                document,
                scene,
                selectionBox,
                terminalSnap,
                cableVertexEdit,
                linhaEndpointEdit,
                retanguloResize,
                circuloResize,
                moveHud,
                alignmentGuides,
                documentSceneSync);
        }

        public static ViewportNavigationService CreateNavigation(Func<ViewportService?> viewportProvider)
        {
            return new ViewportNavigationService(viewportProvider);
        }
    }
}