using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectViewPropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectView _vista;
        private readonly Action<ProjectView, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectViewPropertyCommand(
            AraciDocument document,
            ProjectView vista,
            Action<ProjectView, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _vista = vista ?? throw new ArgumentNullException(nameof(vista));
            _aplicar = aplicar ?? throw new ArgumentNullException(nameof(aplicar));
            _valorAnterior = valorAnterior;
            _valorNovo = valorNovo;
        }

        public void Execute()
        {
            Aplicar(_valorNovo);
        }

        public void Undo()
        {
            Aplicar(_valorAnterior);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(T valor)
        {
            _aplicar(_vista, valor);
            _document.AtualizarPropriedadesVista(_vista);
        }
    }
}
