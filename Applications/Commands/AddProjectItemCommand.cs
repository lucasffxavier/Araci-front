using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class AddProjectItemCommand : IUndoableCommand
    {
        private readonly Action _adicionar;
        private readonly Action _remover;

        private AddProjectItemCommand(Action adicionar, Action remover)
        {
            _adicionar = adicionar ?? throw new ArgumentNullException(nameof(adicionar));
            _remover = remover ?? throw new ArgumentNullException(nameof(remover));
        }

        public static AddProjectItemCommand Vista(AraciDocument document, ProjectView vista)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(vista);

            return new AddProjectItemCommand(
                () => document.AdicionarVista(vista),
                () => document.RemoverVista(vista));
        }

        public static AddProjectItemCommand Tabela(AraciDocument document, ProjectTable tabela)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tabela);

            return new AddProjectItemCommand(
                () => document.AdicionarTabela(tabela),
                () => document.RemoverTabela(tabela));
        }

        public static AddProjectItemCommand Prancha(AraciDocument document, ProjectSheet prancha)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(prancha);

            return new AddProjectItemCommand(
                () => document.AdicionarPrancha(prancha),
                () => document.RemoverPrancha(prancha));
        }

        public static AddProjectItemCommand TipoPrancha(AraciDocument document, ProjectSheetType tipo)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tipo);

            return new AddProjectItemCommand(
                () => document.AdicionarTipoPrancha(tipo),
                () => document.RemoverTipoPrancha(tipo));
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
