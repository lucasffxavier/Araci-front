using System;
using Araci.Models;
using Araci.Services;

namespace Araci.Core.Commands
{
    public class AddElementoCommand : IUndoableCommand
    {
        private readonly Elemento _elemento;
        private readonly EditorContext _context;
        private bool _inicializado;
        private string? _nomeFinal;

        public AddElementoCommand(Elemento elemento, EditorContext context)
        {
            _elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Execute()
        {
            PrepararPrimeiraExecucao();
            _context.Document.AdicionarElemento(_elemento);
        }

        public void Undo()
        {
            _context.Document.RemoverElemento(_elemento);
        }

        public void Redo()
        {
            RestaurarEstadoInicializado();
            _context.Document.AdicionarElemento(_elemento);
        }

        private void PrepararPrimeiraExecucao()
        {
            if (_inicializado)
            {
                RestaurarEstadoInicializado();
                return;
            }

            _context.Names.GarantirNomeUnico(_elemento);
            _nomeFinal = _elemento.Nome;
            _inicializado = true;
        }

        private void RestaurarEstadoInicializado()
        {
            if (_nomeFinal != null)
                _elemento.Nome = _nomeFinal;
        }
    }
}
