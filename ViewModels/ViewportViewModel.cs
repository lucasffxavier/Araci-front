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
            LinhaEndpointEditService linhaEndpointEdit,
            RetanguloResizeService retanguloResize,
            CirculoResizeService circuloResize,
            MoveHudService moveHud,
            AlignmentGuideService alignmentGuides,
            DocumentSceneSyncService documentSceneSync)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            SelectionBox = selectionBox ?? throw new ArgumentNullException(nameof(selectionBox));
            TerminalSnap = terminalSnap ?? throw new ArgumentNullException(nameof(terminalSnap));
            CableVertexEdit = cableVertexEdit ?? throw new ArgumentNullException(nameof(cableVertexEdit));
            LinhaEndpointEdit = linhaEndpointEdit ?? throw new ArgumentNullException(nameof(linhaEndpointEdit));
            RetanguloResize = retanguloResize ?? throw new ArgumentNullException(nameof(retanguloResize));
            CirculoResize = circuloResize ?? throw new ArgumentNullException(nameof(circuloResize));
            MoveHud = moveHud ?? throw new ArgumentNullException(nameof(moveHud));
            ArgumentNullException.ThrowIfNull(alignmentGuides);
            AlignmentGuides = alignmentGuides.Linhas;
            _documentSceneSync = documentSceneSync ?? throw new ArgumentNullException(nameof(documentSceneSync));
            Document.VistaAtivaAlterada += OnVistaAtivaAlterada;
            Document.PropriedadesVistaAlteradas += OnPropriedadesVistaAlteradas;
        }

        public AraciDocument Document { get; }
        public Scene Scene { get; }
        public SelectionBoxViewModel SelectionBox { get; }
        public TerminalSnapState TerminalSnap { get; }
        public CableVertexEditService CableVertexEdit { get; }
        public LinhaEndpointEditService LinhaEndpointEdit { get; }
        public RetanguloResizeService RetanguloResize { get; }
        public CirculoResizeService CirculoResize { get; }
        public MoveHudService MoveHud { get; }
        public ObservableCollection<AlignmentGuideLineViewModel> AlignmentGuides { get; }
        public ObservableCollection<ElementoViewModel> Elementos => Scene.Elementos;
        public double InverseZoom => _inverseZoom;
        public double CableHandleVisualOffset => -5 * _inverseZoom;
        public double LinhaHandleVisualSize => 10 * _inverseZoom;
        public double LinhaHandleVisualOffset => -5 * _inverseZoom;
        public double LinhaHandleStrokeThickness => 1.5 * _inverseZoom;
        public double RetanguloHandleVisualSize => 10 * _inverseZoom;
        public double RetanguloHandleVisualOffset => -5 * _inverseZoom;
        public double RetanguloHandleStrokeThickness => 1.5 * _inverseZoom;
        public double CirculoHandleVisualSize => 10 * _inverseZoom;
        public double CirculoHandleVisualOffset => -5 * _inverseZoom;
        public double CirculoHandleStrokeThickness => 1.5 * _inverseZoom;
        public double TextoHandleVisualSize => 10 * _inverseZoom;
        public double TextoHandleVisualHalfOffset => 5 * _inverseZoom;
        public double TextoHandleVisualNegativeHalfOffset => -5 * _inverseZoom;
        public double TextoHandleStrokeThickness => 1.5 * _inverseZoom;
        public double TerminalMarkerVisualSize => 20 * _inverseZoom;
        public double TerminalMarkerVisualOffset => -10 * _inverseZoom;
        public double TerminalMarkerStrokeThickness => 2 * _inverseZoom;
        public double TerminalMessageVisualOffsetX => 16 * _inverseZoom;
        public double TerminalMessageVisualOffsetY => -34 * _inverseZoom;
        public double TerminalMessageFontSize => 12 * _inverseZoom;
        public Thickness TerminalMessagePadding => new(7 * _inverseZoom, 4 * _inverseZoom, 7 * _inverseZoom, 4 * _inverseZoom);
        public CornerRadius TerminalMessageCornerRadius => new(5 * _inverseZoom);
        public bool RecorteVistaVisivel => Document.VistaAtiva?.RegiaoRecorteVisivel == true;
        public double RecorteVistaX => Document.VistaAtiva?.RecorteX ?? 0.0;
        public double RecorteVistaY => Document.VistaAtiva?.RecorteY ?? 0.0;
        public double RecorteVistaLargura => Math.Max(ProjectView.MinRecorteDimension, Document.VistaAtiva?.RecorteLargura ?? ProjectView.DefaultRecorteLargura);
        public double RecorteVistaAltura => Math.Max(ProjectView.MinRecorteDimension, Document.VistaAtiva?.RecorteAltura ?? ProjectView.DefaultRecorteAltura);
        public double RecorteVistaStrokeThickness => 1.5 * _inverseZoom;
        public double RecorteVistaHandleVisualSize => 10 * _inverseZoom;
        public double RecorteVistaHandleVisualNegativeHalfOffset => -5 * _inverseZoom;
        public double RecorteVistaHandleStrokeThickness => 1.5 * _inverseZoom;
        public double RecorteVistaTopLeftHandleX => RecorteVistaX;
        public double RecorteVistaTopLeftHandleY => RecorteVistaY;
        public double RecorteVistaTopHandleX => RecorteVistaX + RecorteVistaLargura / 2;
        public double RecorteVistaTopHandleY => RecorteVistaY;
        public double RecorteVistaTopRightHandleX => RecorteVistaX + RecorteVistaLargura;
        public double RecorteVistaTopRightHandleY => RecorteVistaY;
        public double RecorteVistaRightHandleX => RecorteVistaX + RecorteVistaLargura;
        public double RecorteVistaRightHandleY => RecorteVistaY + RecorteVistaAltura / 2;
        public double RecorteVistaBottomRightHandleX => RecorteVistaX + RecorteVistaLargura;
        public double RecorteVistaBottomRightHandleY => RecorteVistaY + RecorteVistaAltura;
        public double RecorteVistaBottomHandleX => RecorteVistaX + RecorteVistaLargura / 2;
        public double RecorteVistaBottomHandleY => RecorteVistaY + RecorteVistaAltura;
        public double RecorteVistaBottomLeftHandleX => RecorteVistaX;
        public double RecorteVistaBottomLeftHandleY => RecorteVistaY + RecorteVistaAltura;
        public double RecorteVistaLeftHandleX => RecorteVistaX;
        public double RecorteVistaLeftHandleY => RecorteVistaY + RecorteVistaAltura / 2;

        public void AtualizarZoomVisual(double zoom)
        {
            double inverseZoom = zoom > 0 ? 1 / zoom : 1;

            if (Math.Abs(_inverseZoom - inverseZoom) < 0.000001)
                return;

            _inverseZoom = inverseZoom;
            OnPropertyChanged(nameof(InverseZoom));
            OnPropertyChanged(nameof(CableHandleVisualOffset));
            OnPropertyChanged(nameof(LinhaHandleVisualSize));
            OnPropertyChanged(nameof(LinhaHandleVisualOffset));
            OnPropertyChanged(nameof(LinhaHandleStrokeThickness));
            OnPropertyChanged(nameof(RetanguloHandleVisualSize));
            OnPropertyChanged(nameof(RetanguloHandleVisualOffset));
            OnPropertyChanged(nameof(RetanguloHandleStrokeThickness));
            OnPropertyChanged(nameof(CirculoHandleVisualSize));
            OnPropertyChanged(nameof(CirculoHandleVisualOffset));
            OnPropertyChanged(nameof(CirculoHandleStrokeThickness));
            OnPropertyChanged(nameof(TextoHandleVisualSize));
            OnPropertyChanged(nameof(TextoHandleVisualHalfOffset));
            OnPropertyChanged(nameof(TextoHandleVisualNegativeHalfOffset));
            OnPropertyChanged(nameof(TextoHandleStrokeThickness));
            OnPropertyChanged(nameof(TerminalMarkerVisualSize));
            OnPropertyChanged(nameof(TerminalMarkerVisualOffset));
            OnPropertyChanged(nameof(TerminalMarkerStrokeThickness));
            OnPropertyChanged(nameof(TerminalMessageVisualOffsetX));
            OnPropertyChanged(nameof(TerminalMessageVisualOffsetY));
            OnPropertyChanged(nameof(TerminalMessageFontSize));
            OnPropertyChanged(nameof(TerminalMessagePadding));
            OnPropertyChanged(nameof(TerminalMessageCornerRadius));
            OnPropertyChanged(nameof(RecorteVistaStrokeThickness));
            OnPropertyChanged(nameof(RecorteVistaHandleVisualSize));
            OnPropertyChanged(nameof(RecorteVistaHandleVisualNegativeHalfOffset));
            OnPropertyChanged(nameof(RecorteVistaHandleStrokeThickness));
        }

        private void OnVistaAtivaAlterada()
        {
            NotificarRecorteVistaAlterado();
        }

        private void OnPropriedadesVistaAlteradas(ProjectView vista)
        {
            if (Document.VistaAtivaId.HasValue && Document.VistaAtivaId.Value == vista.Id)
                NotificarRecorteVistaAlterado();
        }

        private void NotificarRecorteVistaAlterado()
        {
            OnPropertyChanged(nameof(RecorteVistaVisivel));
            OnPropertyChanged(nameof(RecorteVistaX));
            OnPropertyChanged(nameof(RecorteVistaY));
            OnPropertyChanged(nameof(RecorteVistaLargura));
            OnPropertyChanged(nameof(RecorteVistaAltura));
            OnPropertyChanged(nameof(RecorteVistaStrokeThickness));
            OnPropertyChanged(nameof(RecorteVistaHandleVisualSize));
            OnPropertyChanged(nameof(RecorteVistaHandleVisualNegativeHalfOffset));
            OnPropertyChanged(nameof(RecorteVistaHandleStrokeThickness));
            OnPropertyChanged(nameof(RecorteVistaTopLeftHandleX));
            OnPropertyChanged(nameof(RecorteVistaTopLeftHandleY));
            OnPropertyChanged(nameof(RecorteVistaTopHandleX));
            OnPropertyChanged(nameof(RecorteVistaTopHandleY));
            OnPropertyChanged(nameof(RecorteVistaTopRightHandleX));
            OnPropertyChanged(nameof(RecorteVistaTopRightHandleY));
            OnPropertyChanged(nameof(RecorteVistaRightHandleX));
            OnPropertyChanged(nameof(RecorteVistaRightHandleY));
            OnPropertyChanged(nameof(RecorteVistaBottomRightHandleX));
            OnPropertyChanged(nameof(RecorteVistaBottomRightHandleY));
            OnPropertyChanged(nameof(RecorteVistaBottomHandleX));
            OnPropertyChanged(nameof(RecorteVistaBottomHandleY));
            OnPropertyChanged(nameof(RecorteVistaBottomLeftHandleX));
            OnPropertyChanged(nameof(RecorteVistaBottomLeftHandleY));
            OnPropertyChanged(nameof(RecorteVistaLeftHandleX));
            OnPropertyChanged(nameof(RecorteVistaLeftHandleY));
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