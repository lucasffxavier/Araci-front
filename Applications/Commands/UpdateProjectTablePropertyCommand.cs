using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectTablePropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectTable _tabela;
        private readonly Action<ProjectTable, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectTablePropertyCommand(
            AraciDocument document,
            ProjectTable tabela,
            Action<ProjectTable, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
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
            _aplicar(_tabela, valor);
            _document.AtualizarPropriedadesTabela(_tabela);
        }
    }
}
