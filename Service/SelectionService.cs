using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Araci.Core.Events;
using Araci.ViewModels;

namespace Araci.Services
{
    public class SelectionService
    {
        private readonly EditorContext _context;
        private readonly ObservableCollection<ElementoViewModel> _selecionados = new();

        public SelectionService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
            _context.Editor.ElementoSelecionado = _selecionados.Count switch
            {
                0 => null,
                1 => _selecionados[0],
                _ => new PropertiesViewModel(_selecionados, _context.Commands)
            };
        }

        private void PublicarAlteracao()
        {
            _context.Events.Publish(new SelecaoAlteradaEvent(_selecionados.ToList()));
            SelectionChanged?.Invoke();
        }
    }
}