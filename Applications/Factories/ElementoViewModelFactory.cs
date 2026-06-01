using System;
using Araci.Applications.Abstractions;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Factories
{
    public class ElementoViewModelFactory : IElementViewModelFactory
    {
        private readonly IElementCatalog _catalog;
        private readonly IElementModelFactory _modelFactory;
        private readonly NameService _names;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly ElementGeometryUpdateService _geometryUpdates;
        private readonly ElementViewModelFactoryContext _context;

        public ElementoViewModelFactory(
            IElementCatalog catalog,
            IElementModelFactory modelFactory,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs,
            TerminalLayoutService terminalLayout,
            ElementGeometryUpdateService geometryUpdates)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _modelFactory = modelFactory ?? throw new ArgumentNullException(nameof(modelFactory));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _geometryUpdates = geometryUpdates ?? throw new ArgumentNullException(nameof(geometryUpdates));
            _context = new ElementViewModelFactoryContext(_names, _typePropertiesDialogs, _terminalLayout);
        }

        public ElementoViewModel? CreateViewModel(Elemento modelo)
        {
            ElementoViewModel? vm = _catalog.FindByKind(_catalog.GetKind(modelo))
                ?.CriarViewModel(modelo, _context);

            ConfigurarViewModel(vm);
            return vm;
        }

        public TViewModel CreateViewModel<TViewModel>(string kind) where TViewModel : ElementoViewModel
        {
            Elemento modelo = _modelFactory.CreateModel(kind);
            ElementoViewModel? viewModel = CreateViewModel(modelo);

            if (viewModel is not TViewModel typed)
                throw new InvalidOperationException($"O elemento '{kind}' nao cria ViewModel do tipo {typeof(TViewModel).Name}.");

            return typed;
        }

        private void ConfigurarViewModel(ElementoViewModel? vm)
        {
            if (vm is BarraViewModel barra)
                barra.GeometryUpdates = _geometryUpdates;
        }
    }
}
