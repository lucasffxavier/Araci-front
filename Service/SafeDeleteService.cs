using System;
using System.Linq;

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

            if (!_context.ExcluirElemento.Executar(selecionados))
                return false;

            LimparEstadoVisual();
            return true;
        }

        private void LimparEstadoVisual()
        {
            _context.Selection.Limpar();
            _context.CableVertexEdit.Clear();
            _context.Hover.Clear();
            _context.TerminalSnap.Limpar();
            _context.SceneQueries.Invalidate();
        }
    }
}
