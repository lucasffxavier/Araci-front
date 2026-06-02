using System;
using System.Collections.Generic;
using Araci.Applications.Abstractions;
using Araci.Applications.Editor;
using Araci.Applications.Editar.Selecionar;
using Araci.Applications.UseCases.Editar;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Events;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.ViewModels;
using Araci.Services.Geometry;
using Araci.Services.Topology;

namespace Araci.Services.Composition
{
    internal static class EditingComposition
    {
        public static VisualUpdateService CreateVisualUpdates(
            Func<ViewportService?> viewportProvider,
            TerminalLayoutService terminalLayout,
            ConnectivityService connectivity,
            ISceneQueryService sceneQueries,
            TerminalSnapState terminalSnap,
            Action refreshCableVertexEdit)
        {
            return new VisualUpdateService(
                viewportProvider,
                terminalLayout,
                connectivity,
                sceneQueries,
                terminalSnap,
                refreshCableVertexEdit);
        }

        public static SelectionService CreateSelection(
            EditorState editor,
            IEventBus events,
            EditarPropriedadesUseCase editarPropriedades)
        {
            return new SelectionService(editor, events, editarPropriedades);
        }

        public static CableVertexEditService CreateCableVertexEdit(
            SelectionService selection,
            ISceneQueryService sceneQueries,
            VisualUpdateService visualUpdates,
            EditarVerticesCaboUseCase editarVerticesCabo)
        {
            return new CableVertexEditService(selection, sceneQueries, visualUpdates, editarVerticesCabo);
        }

        public static SafeDeleteService CreateSafeDelete(
            SelectionService selection,
            CableVertexEditService cableVertexEdit,
            ExcluirElementoUseCase excluirElemento,
            HoverService hover,
            TerminalSnapState terminalSnap,
            ISceneQueryService sceneQueries)
        {
            return new SafeDeleteService(selection, cableVertexEdit, excluirElemento, hover, terminalSnap, sceneQueries);
        }

        public static ClipboardService CreateClipboard(
            CopiarElementosUseCase copiarElementos,
            ColarElementosUseCase colarElementos,
            SelectionService selection,
            Func<ViewportService?> viewportProvider,
            ISceneQueryService sceneQueries,
            CableVertexEditService cableVertexEdit)
        {
            return new ClipboardService(
                copiarElementos,
                colarElementos,
                selection,
                viewportProvider,
                sceneQueries,
                cableVertexEdit);
        }

        public static MoveServices CreateMoveServices(
            Func<ViewportService?> viewportProvider,
            Func<IEnumerable<ElementoViewModel>> sceneElementsProvider,
            EditorSettings settings,
            ConnectivityService connectivity,
            TerminalLayoutService terminalLayout,
            ISceneQueryService sceneQueries,
            VisualUpdateService visualUpdates,
            SelectionService selection,
            ElementGeometryUpdateService geometryUpdates,
            CommandManager commands)
        {
            var moveHud = new MoveHudService(viewportProvider);
            var alignmentGuides = new AlignmentGuideService(sceneElementsProvider);
            var moveConstraints = new MoveConstraintService(settings);
            var moverElemento = new MoverElementoUseCase(commands, visualUpdates.AtualizarElementoMovido);
            var rotacionarElemento = new RotacionarElementoUseCase(commands);
            var redimensionarBarra = new RedimensionarBarraUseCase(commands, geometryUpdates);
            var move = new MoveService(connectivity, terminalLayout, viewportProvider, sceneQueries, moverElemento);
            var barraResize = new BarraResizeService(selection, geometryUpdates, redimensionarBarra);
            var rotation = new RotationService(selection, connectivity, viewportProvider, visualUpdates, rotacionarElemento);

            return new MoveServices(
                moveHud,
                alignmentGuides,
                moveConstraints,
                moverElemento,
                rotacionarElemento,
                redimensionarBarra,
                move,
                barraResize,
                rotation);
        }

        public static InputRouter CreateInput(
            ToolService tools,
            CommandManager commands,
            ISafeDeleteService safeDelete,
            ISelectionService selection,
            IElementCatalog elements,
            IHoverService hover,
            Action copySelected,
            Action paste)
        {
            return new InputRouter(tools, commands, safeDelete, selection, elements, hover, copySelected, paste);
        }
    }

    internal sealed record MoveServices(
        MoveHudService MoveHud,
        AlignmentGuideService AlignmentGuides,
        MoveConstraintService MoveConstraints,
        MoverElementoUseCase MoverElemento,
        RotacionarElementoUseCase RotacionarElemento,
        RedimensionarBarraUseCase RedimensionarBarra,
        MoveService Move,
        BarraResizeService BarraResize,
        RotationService Rotation);
}
