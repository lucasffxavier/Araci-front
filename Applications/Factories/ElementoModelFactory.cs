using System;
using Araci.Applications.Abstractions;
using Araci.Models;

namespace Araci.Applications.Factories
{
    public class ElementoModelFactory : IElementModelFactory
    {
        private readonly IElementCatalog _catalog;

        public ElementoModelFactory(IElementCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public Elemento CreateModel(string kind)
        {
            ElementDefinition definition = _catalog.FindByKind(kind)
                ?? throw new InvalidOperationException($"Elemento nao registrado: {kind}.");

            Elemento elemento = definition.CriarModelo();
            definition.AtualizarTerminais(elemento);
            return elemento;
        }

        public TModel CreateModel<TModel>(string kind) where TModel : Elemento
        {
            Elemento modelo = CreateModel(kind);

            if (modelo is not TModel typed)
                throw new InvalidOperationException($"O elemento '{kind}' nao cria modelo do tipo {typeof(TModel).Name}.");

            return typed;
        }
    }
}
