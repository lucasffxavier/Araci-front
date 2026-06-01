using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Araci.Core.Documents;
using Araci.Core.SceneQueries;
using Araci.Core.Scenes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class ViewportViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<Elemento, ElementoViewModel> _viewModelsPorModelo = new();
        private readonly SelectionService _selection;
        private readonly HoverService _hover;
        private readonly AlignmentGuideService _alignmentGuidesService;
        private readonly ISceneQueryService _sceneQueries;
        private readonly ElementoFactory _elementoFactory;
        private double _inverseZoom = 1;

        public ViewportViewModel(
            AraciDocument document,
            Scene scene,
            SelectionBoxViewModel selectionBox,
            TerminalSnapState terminalSnap,
            CableVertexEditService cableVertexEdit,
            MoveHudService moveHud,
            AlignmentGuideService alignmentGuides,
            SelectionService selection,
            HoverService hover,
            ISceneQueryService sceneQueries,
            ElementoFactory elementoFactory)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            SelectionBox = selectionBox ?? throw new ArgumentNullException(nameof(selectionBox));
            TerminalSnap = terminalSnap ?? throw new ArgumentNullException(nameof(terminalSnap));
            CableVertexEdit = cableVertexEdit ?? throw new ArgumentNullException(nameof(cableVertexEdit));
            MoveHud = moveHud ?? throw new ArgumentNullException(nameof(moveHud));
            _alignmentGuidesService = alignmentGuides ?? throw new ArgumentNullException(nameof(alignmentGuides));
            AlignmentGuides = alignmentGuides.Linhas;
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _hover = hover ?? throw new ArgumentNullException(nameof(hover));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _elementoFactory = elementoFactory ?? throw new ArgumentNullException(nameof(elementoFactory));
            Document.Elementos.CollectionChanged += OnDocumentElementosChanged;
            SincronizarComDocumento();
        }

        public AraciDocument Document { get; }
        public Scene Scene { get; }
        public SelectionBoxViewModel SelectionBox { get; }
        public TerminalSnapState TerminalSnap { get; }
        public CableVertexEditService CableVertexEdit { get; }
        public MoveHudService MoveHud { get; }
        public ObservableCollection<AlignmentGuideLineViewModel> AlignmentGuides { get; }
        public ObservableCollection<ElementoViewModel> Elementos => Scene.Elementos;
        public double InverseZoom => _inverseZoom;
        public double CableHandleVisualOffset => -5 * _inverseZoom;
        public double TerminalMarkerVisualSize => 20 * _inverseZoom;
        public double TerminalMarkerVisualOffset => -10 * _inverseZoom;
        public double TerminalMarkerStrokeThickness => 2 * _inverseZoom;
        public double TerminalMessageVisualOffsetX => 16 * _inverseZoom;
        public double TerminalMessageVisualOffsetY => -34 * _inverseZoom;
        public double TerminalMessageFontSize => 12 * _inverseZoom;
        public Thickness TerminalMessagePadding => new(7 * _inverseZoom, 4 * _inverseZoom, 7 * _inverseZoom, 4 * _inverseZoom);
        public CornerRadius TerminalMessageCornerRadius => new(5 * _inverseZoom);

        public void AtualizarZoomVisual(double zoom)
        {
            double inverseZoom = zoom > 0 ? 1 / zoom : 1;

            if (Math.Abs(_inverseZoom - inverseZoom) < 0.000001)
                return;

            _inverseZoom = inverseZoom;
            OnPropertyChanged(nameof(InverseZoom));
            OnPropertyChanged(nameof(CableHandleVisualOffset));
            OnPropertyChanged(nameof(TerminalMarkerVisualSize));
            OnPropertyChanged(nameof(TerminalMarkerVisualOffset));
            OnPropertyChanged(nameof(TerminalMarkerStrokeThickness));
            OnPropertyChanged(nameof(TerminalMessageVisualOffsetX));
            OnPropertyChanged(nameof(TerminalMessageVisualOffsetY));
            OnPropertyChanged(nameof(TerminalMessageFontSize));
            OnPropertyChanged(nameof(TerminalMessagePadding));
            OnPropertyChanged(nameof(TerminalMessageCornerRadius));
        }

        public void RegistrarViewModel(ElementoViewModel vm)
        {
            if (!Document.Elementos.Contains(vm.Modelo) || !Elementos.Contains(vm))
                return;

            _viewModelsPorModelo[vm.Modelo] = vm;
        }

        public ElementoViewModel? ObterViewModel(Elemento modelo)
        {
            if (!_viewModelsPorModelo.TryGetValue(modelo, out var vm))
                return null;

            if (Document.Elementos.Contains(modelo) && Elementos.Contains(vm))
                return vm;

            _viewModelsPorModelo.Remove(modelo);
            return null;
        }

        public void AtualizarViewModel(Elemento modelo)
        {
            ObterViewModel(modelo)?.AtualizarAposModeloAlterado();
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

        private void SincronizarComDocumento()
        {
            LimparViewModels();

            foreach (Elemento modelo in Document.Elementos)
                AdicionarViewModel(modelo);
        }

        private void AdicionarViewModel(Elemento modelo)
        {
            var vm = ObterOuCriarViewModel(modelo);

            if (vm == null || Elementos.Contains(vm))
                return;

            Elementos.Add(vm);
        }

        private void RemoverViewModel(Elemento modelo)
        {
            if (!_viewModelsPorModelo.TryGetValue(modelo, out var vm))
                return;

            _selection.Deselecionar(vm);
            CableVertexEdit.Clear();
            _hover.Clear();
            Elementos.Remove(vm);
            _viewModelsPorModelo.Remove(modelo);
            _sceneQueries.Invalidate();
        }

        private void LimparViewModels()
        {
            _selection.Limpar();
            CableVertexEdit.Clear();
            _hover.Clear();
            TerminalSnap.Limpar();
            _alignmentGuidesService.Limpar();
            Elementos.Clear();
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }
}
