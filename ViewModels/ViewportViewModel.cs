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

        public ViewportViewModel(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Document = context.Document;
            Scene = context.Scene;
            SelectionBox = context.SelectionBox;
            TerminalSnap = context.TerminalSnap;
            MoveHud = context.MoveHud;

            Document.Elementos.CollectionChanged += OnDocumentElementosChanged;

            SincronizarComDocumento();
        }

        public AraciDocument Document { get; }
        public Scene Scene { get; }
        public SelectionBoxViewModel SelectionBox { get; }
        public TerminalSnapState TerminalSnap { get; }
        public MoveHudService MoveHud { get; }

        public ObservableCollection<ElementoViewModel> Elementos => Scene.Elementos;

        public void RegistrarViewModel(ElementoViewModel vm)
        {
            _viewModelsPorModelo[vm.Modelo] = vm;
        }

        public ElementoViewModel? ObterViewModel(Elemento modelo)
        {
            return _viewModelsPorModelo.TryGetValue(modelo, out var vm)
                ? vm
                : null;
        }

        public void AtualizarViewModel(Elemento modelo)
        {
            ObterViewModel(modelo)?.AtualizarAposModeloAlterado();
        }

        private void OnDocumentElementosChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Elementos.Clear();
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
            Elementos.Clear();

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
            Elementos.Remove(vm);
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
