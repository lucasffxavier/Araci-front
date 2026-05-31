using System;
using System.Collections.Generic;
using Araci.Models;

namespace Araci.Services
{
    public class VisualUpdateService
    {
        private readonly EditorContext _context;

        public VisualUpdateService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AtualizarElementoMovido(Elemento elemento)
        {
            _context.Viewport?.AtualizarViewModel(elemento);
            _context.TerminalLayout.AtualizarTerminais(elemento);
            _context.SceneQueries.Invalidate();
            _context.CableVertexEdit.Refresh();
        }

        public void AtualizarElementoRotacionado(Elemento elemento)
        {
            if (elemento is Cabo cabo)
            {
                AtualizarCabo(cabo);
                AtualizarEstadoRotacao();
                return;
            }

            _context.TerminalLayout.AtualizarTerminais(elemento);

            IReadOnlyList<Cabo> cabosReancorados =
                _context.Connectivity.ReancorarCabosConectados(elemento);

            _context.Viewport?.AtualizarViewModel(elemento);

            foreach (Cabo caboReancorado in cabosReancorados)
                AtualizarCabo(caboReancorado);

            AtualizarEstadoRotacao();
        }

        public void AtualizarCaboEditado(Elemento elemento)
        {
            _context.Viewport?.AtualizarViewModel(elemento);
            _context.SceneQueries.Invalidate();
        }

        public void AtualizarGeometriaElementoECabos(Elemento elemento, IEnumerable<Cabo> cabos)
        {
            _context.Viewport?.AtualizarViewModel(elemento);

            foreach (Cabo cabo in cabos)
                _context.Viewport?.AtualizarViewModel(cabo);

            _context.SceneQueries.Invalidate();
            _context.TerminalSnap.Limpar();
            _context.CableVertexEdit.Refresh();
        }

        private void AtualizarCabo(Cabo cabo)
        {
            _context.TerminalLayout.AtualizarTerminais(cabo);
            _context.Viewport?.AtualizarViewModel(cabo);
        }

        private void AtualizarEstadoRotacao()
        {
            _context.SceneQueries.Invalidate();
            _context.TerminalSnap.Limpar();
            _context.CableVertexEdit.Refresh();
        }
    }
}
