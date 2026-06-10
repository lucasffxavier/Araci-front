using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class DuplicateProjectItemCommand : IUndoableCommand
    {
        private readonly Action _adicionar;
        private readonly Action _remover;

        private DuplicateProjectItemCommand(Action adicionar, Action remover)
        {
            _adicionar = adicionar ?? throw new ArgumentNullException(nameof(adicionar));
            _remover = remover ?? throw new ArgumentNullException(nameof(remover));
        }

        public static DuplicateProjectItemCommand Vista(AraciDocument document, ProjectView duplicata, int indice)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(duplicata);

            return new DuplicateProjectItemCommand(
                () => document.RestaurarVista(duplicata, indice),
                () => document.RemoverVista(duplicata));
        }

        public static DuplicateProjectItemCommand Tabela(AraciDocument document, ProjectTable duplicata, int indice)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(duplicata);

            return new DuplicateProjectItemCommand(
                () => document.RestaurarTabela(duplicata, indice),
                () => document.RemoverTabela(duplicata));
        }

        public static DuplicateProjectItemCommand Prancha(AraciDocument document, ProjectSheet duplicata, int indice)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(duplicata);

            return new DuplicateProjectItemCommand(
                () => document.RestaurarPrancha(duplicata, indice),
                () => document.RemoverPrancha(duplicata));
        }

        public static DuplicateProjectItemCommand TipoPrancha(AraciDocument document, ProjectSheetType duplicata, int indice)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(duplicata);

            return new DuplicateProjectItemCommand(
                () => document.RestaurarTipoPrancha(duplicata, indice),
                () => document.RemoverTipoPrancha(duplicata));
        }

        public void Execute()
        {
            _adicionar();
        }

        public void Undo()
        {
            _remover();
        }

        public void Redo()
        {
            Execute();
        }
    }
}
