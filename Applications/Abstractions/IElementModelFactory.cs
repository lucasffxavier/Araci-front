using Araci.Models;

namespace Araci.Applications.Abstractions
{
    public interface IElementModelFactory
    {
        Elemento CreateModel(string kind);
        TModel CreateModel<TModel>(string kind) where TModel : Elemento;
    }
}
