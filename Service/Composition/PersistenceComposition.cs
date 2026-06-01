using System;
using Araci.Applications.Abstractions;
using Araci.Applications.Projects;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Infrastructure.Persistence;

namespace Araci.Services.Composition
{
    internal static class PersistenceComposition
    {
        public static ProjectPersistenceService CreateProjects(
            AraciDocument document,
            CommandManager commands,
            ElementRegistryService elements,
            IElementModelFactory modelFactory,
            TerminalLayoutService terminalLayout,
            ElementGeometryService geometry,
            DialogService dialogs,
            Action clearTransientState)
        {
            var serializer = new ProjectSerializer(elements, modelFactory, terminalLayout, geometry);
            var repository = new FileSystemProjectRepository();
            var fileDialogs = new ProjectFileDialogService();

            return new ProjectPersistenceService(
                document,
                commands,
                serializer,
                repository,
                fileDialogs,
                dialogs,
                clearTransientState);
        }
    }
}
