using System;
using Araci.Models;

namespace Araci.Services
{
    public class ElementGeometryUpdateService
    {
        private readonly EditorContext _context;

        public ElementGeometryUpdateService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AplicarAlturaBarra(Barra barra, double altura)
        {
            if (barra == null)
                throw new ArgumentNullException(nameof(barra));

            barra.Altura = altura;
            AtualizarElementoECabos(barra);
        }

        public void AtualizarElementoECabos(Elemento elemento)
        {
            if (elemento == null)
                throw new ArgumentNullException(nameof(elemento));

            _context.TerminalLayout.AtualizarTerminais(elemento);

            var cabos = _context.Connectivity.ReancorarCabosConectados(elemento);

            _context.Viewport?.AtualizarViewModel(elemento);

            foreach (Cabo cabo in cabos)
            {
                _context.TerminalLayout.AtualizarTerminais(cabo);
                _context.Viewport?.AtualizarViewModel(cabo);
            }

            _context.SceneQueries.Invalidate();
            _context.TerminalSnap.Limpar();
            _context.CableVertexEdit.Refresh();
        }
    }
}
