using System;
using Araci.Models;
using Araci.Services;

namespace Araci.Core.Commands
{
    public class AddElementoCommand : IUndoableCommand
    {
        private readonly Elemento _elemento;
        private readonly EditorContext _context;

        public AddElementoCommand(Elemento elemento, EditorContext context)
        {
            _elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Execute()
        {
            _context.Document.AdicionarElemento(_elemento);
        }

        public void Undo()
        {
            _context.Document.RemoverElemento(_elemento);
        }

        public void Redo()
        {
            Execute();
        }
    }
}
