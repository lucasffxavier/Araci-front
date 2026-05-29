using System;
using System.Collections.Generic;
using Araci.Services;

namespace Araci.ViewModels
{
    public static class InstancePropertyCatalog
    {
        private static ElementRegistryService? _registry;

        public static void Configure(ElementRegistryService registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public static IReadOnlyList<InstancePropertyDescriptor> GetFor(ElementoViewModel elemento)
        {
            return _registry?.GetInstanceProperties(elemento) ?? Array.Empty<InstancePropertyDescriptor>();
        }

        public static IReadOnlyList<InstancePropertyDescriptor> GetFor(Type type)
        {
            return _registry?.GetInstanceProperties(type) ?? Array.Empty<InstancePropertyDescriptor>();
        }

        public static IReadOnlyList<InstancePropertyDescriptor> GetCommonFor(IReadOnlyList<ElementoViewModel> elementos)
        {
            return _registry?.GetCommonInstanceProperties(elementos) ?? Array.Empty<InstancePropertyDescriptor>();
        }

        public static bool CanEditAcrossMixedTypes(IReadOnlyList<ElementoViewModel> elementos, string propertyName)
        {
            return _registry?.CanEditAcrossMixedTypes(elementos, propertyName) ?? false;
        }
    }
}