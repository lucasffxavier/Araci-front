using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Araci.Core.Documents;
using Araci.Core.SceneQueries;
using Araci.Applications.Editar.Selecionar;
using Araci.Applications.Factories;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;
using CoreScene = Araci.Core.Scenes.Scene;

namespace Araci.Applications.Scene
{
    public class DocumentSceneSyncService
    {
        private readonly AraciDocument _document;
        private readonly CoreScene _scene;
        private readonly ElementoFactory _elementoFactory;
        private readonly SelectionService _selection;
        private readonly CableVertexEditService _cableVertexEdit;
        private readonly TerminalSnapState _terminalSnap;
        private readonly AlignmentGuideService _alignmentGuides;
        private readonly HoverService _hover;
        private readonly ISceneQueryService _sceneQueries;
        private readonly Dictionary<Elemento, ElementoViewModel> _viewModelsPorModelo = new();

        public DocumentSceneSyncService(
            AraciDocument document,
            CoreScene scene,
            ElementoFactory elementoFactory,
            SelectionService selection,
            CableVertexEditService cableVertexEdit,
            TerminalSnapState terminalSnap,
            AlignmentGuideService alignmentGuides,
            HoverService hover,
            ISceneQueryService sceneQueries)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _elementoFactory = elementoFactory ?? throw new ArgumentNullException(nameof(elementoFactory));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _cableVertexEdit = cableVertexEdit ?? throw new ArgumentNullException(nameof(cableVertexEdit));
            _terminalSnap = terminalSnap ?? throw new ArgumentNullException(nameof(terminalSnap));
            _alignmentGuides = alignmentGuides ?? throw new ArgumentNullException(nameof(alignmentGuides));
            _hover = hover ?? throw new ArgumentNullException(nameof(hover));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));

            _document.Elementos.CollectionChanged += OnDocumentElementosChanged;
            SincronizarComDocumento();
        }

        public void RegistrarViewModel(ElementoViewModel vm)
        {
            if (!_document.Elementos.Contains(vm.Modelo) || !_scene.Elementos.Contains(vm))
                return;

            _viewModelsPorModelo[vm.Modelo] = vm;
        }

        public ElementoViewModel? ObterViewModel(Elemento modelo)
        {
            if (!_viewModelsPorModelo.TryGetValue(modelo, out var vm))
                return null;

            if (_document.Elementos.Contains(modelo) && _scene.Elementos.Contains(vm))
                return vm;

            _viewModelsPorModelo.Remove(modelo);
            return null;
        }

        public void AtualizarViewModel(Elemento modelo)
        {
            ObterViewModel(modelo)?.AtualizarAposModeloAlterado();
        }

        public void SincronizarComDocumento()
        {
            LimparViewModels();

            foreach (Elemento modelo in _document.Elementos)
                AdicionarViewModel(modelo);
        }

        private void OnDocumentElementosChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                LimparViewModels();
                return;
            }

            if (e.OldItems != null)
            {
                foreach (Elemento modelo in e.OldItems)
                    RemoverViewModel(modelo);
            }

            if (e.NewItems != null)
            {
                foreach (Elemento modelo in e.NewItems)
                    AdicionarViewModel(modelo);
            }
        }

        private void AdicionarViewModel(Elemento modelo)
        {
            var vm = ObterOuCriarViewModel(modelo);

            if (vm == null || _scene.Elementos.Contains(vm))
                return;

            _scene.Elementos.Add(vm);
        }

        private void RemoverViewModel(Elemento modelo)
        {
            if (!_viewModelsPorModelo.TryGetValue(modelo, out var vm))
                return;

            _selection.Deselecionar(vm);
            _cableVertexEdit.Clear();
            _hover.Clear();
            _scene.Elementos.Remove(vm);
            _viewModelsPorModelo.Remove(modelo);
            _sceneQueries.Invalidate();
        }

        private void LimparViewModels()
        {
            _selection.Limpar();
            _cableVertexEdit.Clear();
            _hover.Clear();
            _terminalSnap.Limpar();
            _alignmentGuides.Limpar();
            _scene.Elementos.Clear();
            _viewModelsPorModelo.Clear();
            _sceneQueries.Invalidate();
        }

        private ElementoViewModel? ObterOuCriarViewModel(Elemento modelo)
        {
            if (_viewModelsPorModelo.TryGetValue(modelo, out var existente))
                return existente;

            var vm = _elementoFactory.CriarViewModel(modelo);

            if (vm != null)
                _viewModelsPorModelo[modelo] = vm;

            return vm;
        }
    }
}
