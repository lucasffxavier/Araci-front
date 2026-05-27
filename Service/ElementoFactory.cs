using System;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ElementoFactory
    {
        private readonly ElementRegistryService _registry;
        private readonly NameService _names;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;

        public ElementoFactory(
            ElementRegistryService registry,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));
        }

        public ElementoViewModel? CriarViewModel(Elemento modelo)
        {
            return _registry.CreateViewModel(modelo, _names, _typePropertiesDialogs);
        }

        public Cabo CriarCabo()
        {
            return _registry.CreateModel<Cabo>();
        }

        public CaboViewModel CriarCaboVM()
        {
            return (CaboViewModel)CriarViewModel(CriarCabo())!;
        }

        public Carga CriarCarga()
        {
            return _registry.CreateModel<Carga>();
        }

        public CargaViewModel CriarCargaVM()
        {
            return (CargaViewModel)CriarViewModel(CriarCarga())!;
        }

        public Gerador CriarGerador()
        {
            return _registry.CreateModel<Gerador>();
        }

        public GeradorViewModel CriarGeradorVM()
        {
            return (GeradorViewModel)CriarViewModel(CriarGerador())!;
        }

        public Barra CriarBarra()
        {
            return _registry.CreateModel<Barra>();
        }

        public BarraViewModel CriarBarraVM()
        {
            return (BarraViewModel)CriarViewModel(CriarBarra())!;
        }
    }
}
