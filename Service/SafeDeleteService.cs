using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Core.Commands;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class SafeDeleteService
    {
        private readonly EditorContext _context;

        public SafeDeleteService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool DeleteActiveHandleOrSelection()
        {
            if (_context.CableVertexEdit.TryRemoveActive())
                return true;

            return DeleteSelection();
        }

        public bool DeleteSelection()
        {
            var selecionados = _context.Selection.Selecionados.ToList();

            if (selecionados.Count == 0)
                return false;

            var elementos = ColetarElementosParaExcluir(selecionados)
                .OrderBy(e => e is Cabo ? 0 : 1)
                .ToList();

            if (elementos.Count == 0)
                return false;

            using var tx = _context.BeginTransaction();

            foreach (Elemento elemento in elementos)
                tx.Add(new DeleteElementCommand(elemento, _context));

            tx.Commit();
            LimparEstadoVisual();

            return true;
        }

        private IEnumerable<Elemento> ColetarElementosParaExcluir(
            IEnumerable<ElementoViewModel> selecionados)
        {
            var resultado = new List<Elemento>();
            var ids = new HashSet<Guid>();

            foreach (ElementoViewModel vm in selecionados)
            {
                Adicionar(vm.Modelo, resultado, ids);

                if (vm.Modelo is Cabo)
                    continue;

                foreach (Cabo cabo in _context.Connectivity.ObterCabosConectados(vm.Modelo))
                    Adicionar(cabo, resultado, ids);
            }

            return resultado;
        }

        private void LimparEstadoVisual()
        {
            _context.Selection.Limpar();
            _context.CableVertexEdit.Clear();
            _context.Hover.Clear();
            _context.TerminalSnap.Limpar();
            _context.SceneQueries.Invalidate();
        }

        private static void Adicionar(
            Elemento elemento,
            ICollection<Elemento> resultado,
            ISet<Guid> ids)
        {
            if (ids.Add(elemento.Id))
                resultado.Add(elemento);
        }
    }
}
