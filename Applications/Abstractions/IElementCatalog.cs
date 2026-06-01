using Araci.Models;

namespace Araci.Applications.Abstractions
{
    public interface IElementCatalog
    {
        ElementDefinition? FindByKind(string kind);
        ElementDefinition? FindByShortcut(string shortcut);
        string GetKind(Elemento elemento);
    }
}
