using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Araci.Applications.Editar.Selecionar;
using Araci.Applications.Scene;
using Araci.Core.Documents;
using Araci.Core.Scenes;
using Araci.Models;
using Araci.Services;
using Araci.Services.Editing;

namespace Araci.ViewModels
{
    public class ViewportViewModel : INotifyPropertyChanged
    {
        private readonly DocumentSceneSyncService _documentSceneSync;
        private double _inverseZoom = 1;

        public ViewportViewModel(
            AraciDocument document,
            Scene scene,
            SelectionBoxViewModel selectionBox,
            TerminalSnapState terminalSnap,
            CableVertexEditService cableVertexEdit,
            MoveHudService moveHud,
            AlignmentGuideService alignmentGuides,
            DocumentSceneSyncService documentSceneSync)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            SelectionBox = selectionBox ?? throw new ArgumentNullException(nameof(selectionBox));
            TerminalSnap = terminalSnap ?? throw new ArgumentNullException(nameof(terminalSnap));
            CableVertexEdit = cableVertexEdit ?? throw new ArgumentNullException(nameof(cableVertexEdit));
            MoveHud = moveHud ?? throw new ArgumentNullException(nameof(moveHud));
            ArgumentNullException.ThrowIfNull(alignmentGuides);
            AlignmentGuides = alignmentGuides.Linhas;
            _documentSceneSync = documentSceneSync ?? throw new ArgumentNullException(nameof(documentSceneSync));
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
            _documentSceneSync.RegistrarViewModel(vm);
        }

        public ElementoViewModel? ObterViewModel(Elemento modelo)
        {
            return _documentSceneSync.ObterViewModel(modelo);
        }

        public void AtualizarViewModel(Elemento modelo)
        {
            _documentSceneSync.AtualizarViewModel(modelo);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }
}
