using System;
using Araci.Applications.Abstractions;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Applications.Factories
{
    public class ElementoFactory
    {
        private readonly IElementModelFactory _modelFactory;
        private readonly IElementViewModelFactory _viewModelFactory;

        public ElementoFactory(IElementModelFactory modelFactory, IElementViewModelFactory viewModelFactory)
        {
            _modelFactory = modelFactory ?? throw new ArgumentNullException(nameof(modelFactory));
            _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
        }

        public Elemento CriarModelo(string kind)
        {
            return _modelFactory.CreateModel(kind);
        }

        public TModel CriarModelo<TModel>(string kind) where TModel : Elemento
        {
            return _modelFactory.CreateModel<TModel>(kind);
        }

        public ElementoViewModel? CriarViewModel(Elemento modelo)
        {
            return _viewModelFactory.CreateViewModel(modelo);
        }

        public TViewModel CriarViewModel<TViewModel>(string kind) where TViewModel : ElementoViewModel
        {
            return _viewModelFactory.CreateViewModel<TViewModel>(kind);
        }

        public ElementoViewModel CriarViewModel(string kind)
        {
            Elemento modelo = CriarModelo(kind);
            return CriarViewModel(modelo) ?? throw new InvalidOperationException($"Nao foi possivel criar ViewModel para o elemento '{kind}'.");
        }

        public Cabo CriarCabo()
        {
            return CriarModelo<Cabo>(ElementKinds.Cabo);
        }

        public CaboViewModel CriarCaboVM()
        {
            return CriarViewModel<CaboViewModel>(ElementKinds.Cabo);
        }

        public Carga CriarCarga()
        {
            return CriarModelo<Carga>(ElementKinds.Carga);
        }

        public CargaViewModel CriarCargaVM()
        {
            return CriarViewModel<CargaViewModel>(ElementKinds.Carga);
        }

        public Gerador CriarGerador()
        {
            return CriarModelo<Gerador>(ElementKinds.Gerador);
        }

        public GeradorViewModel CriarGeradorVM()
        {
            return CriarViewModel<GeradorViewModel>(ElementKinds.Gerador);
        }

        public Sin CriarSin()
        {
            return CriarModelo<Sin>(ElementKinds.Sin);
        }

        public SinViewModel CriarSinVM()
        {
            return CriarViewModel<SinViewModel>(ElementKinds.Sin);
        }

        public Transformador CriarTransformador()
        {
            return CriarModelo<Transformador>(ElementKinds.Transformador);
        }

        public TransformadorViewModel CriarTransformadorVM()
        {
            return CriarViewModel<TransformadorViewModel>(ElementKinds.Transformador);
        }

        public Barra CriarBarra()
        {
            return CriarModelo<Barra>(ElementKinds.Barra);
        }

        public BarraViewModel CriarBarraVM()
        {
            return CriarViewModel<BarraViewModel>(ElementKinds.Barra);
        }

        public LinhaAnotativa CriarLinhaAnotativa()
        {
            return CriarModelo<LinhaAnotativa>(ElementKinds.LinhaAnotativa);
        }

        public LinhaAnotativaViewModel CriarLinhaAnotativaVM()
        {
            return CriarViewModel<LinhaAnotativaViewModel>(ElementKinds.LinhaAnotativa);
        }

        public RetanguloAnotativo CriarRetanguloAnotativo()
        {
            return CriarModelo<RetanguloAnotativo>(ElementKinds.RetanguloAnotativo);
        }

        public RetanguloAnotativoViewModel CriarRetanguloAnotativoVM()
        {
            return CriarViewModel<RetanguloAnotativoViewModel>(ElementKinds.RetanguloAnotativo);
        }

        public CirculoAnotativo CriarCirculoAnotativo()
        {
            return CriarModelo<CirculoAnotativo>(ElementKinds.CirculoAnotativo);
        }

        public CirculoAnotativoViewModel CriarCirculoAnotativoVM()
        {
            return CriarViewModel<CirculoAnotativoViewModel>(ElementKinds.CirculoAnotativo);
        }

        public TextoAnotativo CriarTextoAnotativo()
        {
            return CriarModelo<TextoAnotativo>(ElementKinds.TextoAnotativo);
        }

        public TextoAnotativoViewModel CriarTextoAnotativoVM()
        {
            return CriarViewModel<TextoAnotativoViewModel>(ElementKinds.TextoAnotativo);
        }
    }
}