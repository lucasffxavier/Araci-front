using Araci.Core.Documents;
using Araci.Core.SceneQueries;
using Araci.Applications.Factories;
using Araci.Applications.Simulation;
using Araci.ViewModels;
using CoreScene = Araci.Core.Scenes.Scene;
using Araci.Services.Geometry;
using Araci.Services.Topology;

namespace Araci.Services.Composition
{
    internal static class EditorCoreComposition
    {
        public static EditorCoreComponents Create(
            AraciDocument document,
            EditorSettings settings,
            TypeLibraryService types)
        {
            var scene = new CoreScene();
            var sceneQueries = new SceneQueryService(scene);
            var hover = new HoverService(sceneQueries);
            var snap = new SnapService(sceneQueries, settings);
            var typePropertiesDialogs = new TypePropertiesDialogService();
            var dialogs = new DialogService();
            var instanceProperties = new ElementInstancePropertyProvider();
            var definitions = new ElementDefinitionsProvider(types, instanceProperties);
            var elements = new ElementRegistryService(definitions.CreateDefaults());

            InstancePropertyCatalog.Configure(elements);

            var connectivity = new ConnectivityService(document);
            var electricGraph = new ElectricGraphBuilder(document, elements);
            var operationalState = new OperationalGraphStateBuilder();
            var topology = new TopologyValidator(document, connectivity, electricGraph);
            var geometry = new ElementGeometryService(elements);
            var terminalLayout = new TerminalLayoutService(elements, geometry);

            return new EditorCoreComponents(
                scene,
                sceneQueries,
                hover,
                snap,
                typePropertiesDialogs,
                dialogs,
                elements,
                connectivity,
                electricGraph,
                operationalState,
                topology,
                geometry,
                terminalLayout);
        }
    }

    internal sealed record EditorCoreComponents(
        CoreScene Scene,
        ISceneQueryService SceneQueries,
        HoverService Hover,
        SnapService Snap,
        TypePropertiesDialogService TypePropertiesDialogs,
        DialogService Dialogs,
        ElementRegistryService Elements,
        ConnectivityService Connectivity,
        ElectricGraphBuilder ElectricGraph,
        OperationalGraphStateBuilder OperationalState,
        TopologyValidator Topology,
        ElementGeometryService Geometry,
        TerminalLayoutService TerminalLayout);
}
