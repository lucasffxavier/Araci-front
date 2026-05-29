using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Araci.Core.Documents;
using Araci.Core.Scenes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class ViewportViewModel : INotifyPropertyChanged
    {
        private readonly EditorContext _context;
        private readonly Dictionary<Elemento, ElementoViewModel> _viewModelsPorModelo = new();
        private double _inverseZoom = 1;

        public ViewportViewModel(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Document = context.Document;
            Scene = context.Scene;
            SelectionBox = context.SelectionBox;
            TerminalSnap = context.TerminalSnap;
            CableVertexEdit = context.CableVertexEdit;
            MoveHud = context.MoveHud;
            AlignmentGuides = context.AlignmentGuides.Linhas;
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
        public double TerminalMarkerVisualOffset => -7 * _inverseZoom;

        public void AtualizarZoomVisual(double zoom)
        {
            double inverseZoom = zoom > 0 ? 1 / zoom : 1;

            if (Math.Abs(_inverseZoom - inverseZoom) < 0.000001)
                return;

            _inverseZoom = inverseZoom;
            OnPropertyChanged(nameof(InverseZoom));
            OnPropertyChanged(nameof(CableHandleVisualOffset));
            OnPropertyChanged(nameof(TerminalMarkerVisualOffset));
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

            _context.Selection.Deselecionar(vm);
            _context.CableVertexEdit.Clear();
            _context.Hover.Clear();
            Elementos.Remove(vm);
            _viewModelsPorModelo.Remove(modelo);
            _context.SceneQueries.Invalidate();
        }

        private void LimparViewModels()
        {
            _context.Selection.Limpar();
            _context.CableVertexEdit.Clear();
            _context.Hover.Clear();
            _context.TerminalSnap.Limpar();
            _context.AlignmentGuides.Limpar();
            Elementos.Clear();
            _viewModelsPorModelo.Clear();
            _context.SceneQueries.Invalidate();
        }

        private ElementoViewModel? ObterOuCriarViewModel(Elemento modelo)
        {
            if (_viewModelsPorModelo.TryGetValue(modelo, out var existente))
                return existente;

            var vm = _context.ElementoFactory.CriarViewModel(modelo);

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