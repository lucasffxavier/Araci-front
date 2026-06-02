using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Applications.UseCases.Editar;
using Araci.Core.Events;
using Araci.ViewModels;

namespace Araci.Services.Editing
{
    public class SelectionService : ISelectionService
    {
        private readonly EditorState _editorState;
        private readonly IEventBus _events;
        private readonly EditarPropriedadesUseCase _editarPropriedades;
        private readonly ObservableCollection<ElementoViewModel> _selecionados = new();

        public SelectionService(
            EditorState editorState,
            IEventBus events,
            EditarPropriedadesUseCase editarPropriedades)
        {
            _editorState = editorState ?? throw new ArgumentNullException(nameof(editorState));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _editarPropriedades = editarPropriedades ?? throw new ArgumentNullException(nameof(editarPropriedades));
        }

        public IReadOnlyList<ElementoViewModel> Selecionados => _selecionados;
        public ObservableCollection<ElementoViewModel> SelecionadosObservable => _selecionados;
        public bool TemSelecionados => _selecionados.Count > 0;
        public event Action? SelectionChanged;

        public void Selecionar(ElementoViewModel vm, bool adicionarAoExistente = false)
        {
            if (!adicionarAoExistente)
                Limpar();

            if (_selecionados.Contains(vm))
            {
                AtualizarElementoSelecionado();
                return;
            }

            vm.IsSelecionado = true;
            _selecionados.Add(vm);
            AtualizarElementoSelecionado();
            PublicarAlteracao();
        }

        public void Toggle(ElementoViewModel vm)
        {
            if (_selecionados.Contains(vm))
            {
                Deselecionar(vm);
                return;
            }

            Selecionar(vm, true);
        }

        public void Deselecionar(ElementoViewModel vm)
        {
            if (!_selecionados.Contains(vm))
                return;

            vm.IsSelecionado = false;
            _selecionados.Remove(vm);
            AtualizarElementoSelecionado();
            PublicarAlteracao();
        }

        public void Limpar()
        {
            if (_selecionados.Count == 0)
                return;

            foreach (var vm in _selecionados)
                vm.IsSelecionado = false;

            _selecionados.Clear();
            AtualizarElementoSelecionado();
            PublicarAlteracao();
        }

        private void AtualizarElementoSelecionado()
        {
            _editorState.ElementoSelecionado = _selecionados.Count switch
            {
                0 => null,
                1 => _selecionados[0],
                _ => new PropertiesViewModel(_selecionados, _editarPropriedades)
            };
        }

        private void PublicarAlteracao()
        {
            _events.Publish(new SelecaoAlteradaEvent(_selecionados.ToList()));
            SelectionChanged?.Invoke();
        }
    }
}
