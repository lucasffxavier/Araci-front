using System;

using Araci.Core.Events;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class DeleteElementCommand : IUndoableCommand
    {
        private readonly ElementoViewModel _vm;

        private readonly EditorContext _context;

        public DeleteElementCommand(
            ElementoViewModel vm,
            EditorContext context)
        {
            _vm = vm
                ?? throw new ArgumentNullException(nameof(vm));

            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public void Execute()
        {
            _context.Selection.Deselecionar(_vm);
            _context.Document.RemoverElemento(_vm.Modelo);
            _context.Events.Publish(new ElementoRemovidoEvent(_vm));
        }

        public void Undo()
        {
            _context.Document.AdicionarElemento(_vm.Modelo);

            _context.Events.Publish(
                new ElementoAdicionadoEvent(_vm));
        }

        public void Redo()
        {
            Execute();
        }
    }
}