using Araci.Core.Events;
using Araci.ViewModels;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Araci.Services
{
    public class SelectionService
    {
        // =====================================================
        // DEPENDÊNCIAS
        // =====================================================

        private readonly EditorContext _context;

        // =====================================================
        // ESTADO
        // =====================================================

        private readonly ObservableCollection<ElementoViewModel>
            _selecionados = new();

        // =====================================================
        // CONSTRUTOR
        // =====================================================

        public SelectionService(EditorContext context)
        {
            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
        }

        // =====================================================
        // LEITURA
        // =====================================================

        public IReadOnlyList<ElementoViewModel>
            Selecionados =>
                _selecionados;

        public ObservableCollection<ElementoViewModel>
            SelecionadosObservable =>
                _selecionados;

        public bool TemSelecionados =>
            _selecionados.Count > 0;

        // =====================================================
        // SELECIONAR
        // =====================================================

        public void Selecionar(
            ElementoViewModel vm,
            bool adicionarAoExistente = false)
        {
            if (!adicionarAoExistente)
            {
                Limpar();
            }

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

        // =====================================================
        // TOGGLE
        // =====================================================

        public void Toggle(ElementoViewModel vm)
        {
            if (_selecionados.Contains(vm))
            {
                Deselecionar(vm);
                return;
            }

            Selecionar(vm, true);
        }

        // =====================================================
        // DESELECIONAR
        // =====================================================

        public void Deselecionar(ElementoViewModel vm)
        {
            if (!_selecionados.Contains(vm))
                return;

            vm.IsSelecionado = false;

            _selecionados.Remove(vm);

            AtualizarElementoSelecionado();

            PublicarAlteracao();
        }

        // =====================================================
        // LIMPAR
        // =====================================================

        public void Limpar()
        {
            if (_selecionados.Count == 0)
                return;

            foreach (var vm in _selecionados)
            {
                vm.IsSelecionado = false;
            }

            _selecionados.Clear();

            AtualizarElementoSelecionado();

            PublicarAlteracao();
        }

        // =====================================================
        // AUXILIARES
        // =====================================================

        private void AtualizarElementoSelecionado()
        {
            _context.Editor.ElementoSelecionado =
                _selecionados.LastOrDefault();
        }

        private void PublicarAlteracao()
        {
            _context.Events.Publish(
                new SelecaoAlteradaEvent(
                    _selecionados.ToList()));
        }
    }
}