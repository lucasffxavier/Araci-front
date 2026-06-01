using Araci.Core.Documents;
using Araci.Core.SceneQueries;
using CoreScene = Araci.Core.Scenes.Scene;

namespace Araci.Applications.Abstractions
{
    public interface IEditorSession
    {
        AraciDocument Document { get; }

        CoreScene Scene { get; }

        ISceneQueryService SceneQueries { get; }

        ICommandHistory Commands { get; }
    }
}
