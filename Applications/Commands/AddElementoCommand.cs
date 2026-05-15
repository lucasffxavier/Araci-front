using System;

using Araci.Core.Events;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class AddElementoCommand : IUndoableCommand
    {
        private readonly ElementoViewModel _elemento;

        private readonly EditorContext _context;

        public AddElementoCommand(
            ElementoViewModel elemento,
            EditorContext context)
        {
            _elemento = elemento
                ?? throw new ArgumentNullException(nameof(elemento));

            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public void Execute()
        {
            _context.Document.AdicionarElemento(_elemento.Modelo);

            _context.Events.Publish(
                new ElementoAdicionadoEvent(_elemento));
        }

        public void Undo()
        {
            _context.Selection.Deselecionar(_elemento);

            _context.Document.RemoverElemento(_elemento.Modelo);

            _context.Events.Publish(
                new ElementoRemovidoEvent(_elemento));
        }

        public void Redo()
        {
            Execute();
        }
    }
}