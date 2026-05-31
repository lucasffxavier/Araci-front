using Araci.Core.Documents;
using Araci.Core.SceneQueries;
using Araci.Core.Scenes;

namespace Araci.Applications.Abstractions
{
    public interface IEditorSession
    {
        AraciDocument Document { get; }

        Scene Scene { get; }

        ISceneQueryService SceneQueries { get; }

        ICommandHistory Commands { get; }
    }
}
