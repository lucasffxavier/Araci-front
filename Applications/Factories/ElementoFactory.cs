using System;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Factories
{
    public class ElementoFactory
    {
        private readonly ElementRegistryService _registry;
        private readonly NameService _names;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly ElementGeometryUpdateService _geometryUpdates;

        public ElementoFactory(
            ElementRegistryService registry,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs,
            TerminalLayoutService terminalLayout,
            ElementGeometryUpdateService geometryUpdates)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _geometryUpdates = geometryUpdates ?? throw new ArgumentNullException(nameof(geometryUpdates));
        }

        public Elemento CriarModelo(string kind)
        {
            return _registry.CreateModel(kind);
        }

        public TModel CriarModelo<TModel>(string kind) where TModel : Elemento
        {
            return _registry.CreateModel<TModel>(kind);
        }

        public ElementoViewModel? CriarViewModel(Elemento modelo)
        {
            ElementoViewModel? vm = _registry.CreateViewModel(modelo, _names, _typePropertiesDialogs, _terminalLayout);
            ConfigurarViewModel(vm);
            return vm;
        }

        public TViewModel CriarViewModel<TViewModel>(string kind) where TViewModel : ElementoViewModel
        {
            TViewModel vm = _registry.CreateViewModel<TViewModel>(kind, _names, _typePropertiesDialogs, _terminalLayout);
            ConfigurarViewModel(vm);
            return vm;
        }

        public ElementoViewModel CriarViewModel(string kind)
        {
            Elemento modelo = CriarModelo(kind);
            return CriarViewModel(modelo) ?? throw new InvalidOperationException($"Nao foi possivel criar ViewModel para o elemento '{kind}'.");
        }

        public Cabo CriarCabo()
        {
            return CriarModelo<Cabo>(ElementRegistryService.KindCabo);
        }

        public CaboViewModel CriarCaboVM()
        {
            return CriarViewModel<CaboViewModel>(ElementRegistryService.KindCabo);
        }

        public Carga CriarCarga()
        {
            return CriarModelo<Carga>(ElementRegistryService.KindCarga);
        }

        public CargaViewModel CriarCargaVM()
        {
            return CriarViewModel<CargaViewModel>(ElementRegistryService.KindCarga);
        }

        public Gerador CriarGerador()
        {
            return CriarModelo<Gerador>(ElementRegistryService.KindGerador);
        }

        public GeradorViewModel CriarGeradorVM()
        {
            return CriarViewModel<GeradorViewModel>(ElementRegistryService.KindGerador);
        }

        public Sin CriarSin()
        {
            return CriarModelo<Sin>(ElementRegistryService.KindSin);
        }

        public SinViewModel CriarSinVM()
        {
            return CriarViewModel<SinViewModel>(ElementRegistryService.KindSin);
        }

        public Transformador CriarTransformador()
        {
            return CriarModelo<Transformador>(ElementRegistryService.KindTransformador);
        }

        public TransformadorViewModel CriarTransformadorVM()
        {
            return CriarViewModel<TransformadorViewModel>(ElementRegistryService.KindTransformador);
        }

        public Barra CriarBarra()
        {
            return CriarModelo<Barra>(ElementRegistryService.KindBarra);
        }

        public BarraViewModel CriarBarraVM()
        {
            return CriarViewModel<BarraViewModel>(ElementRegistryService.KindBarra);
        }

        private void ConfigurarViewModel(ElementoViewModel? vm)
        {
            if (vm is BarraViewModel barra)
                barra.GeometryUpdates = _geometryUpdates;
        }
    }
}
