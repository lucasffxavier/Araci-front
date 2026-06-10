using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class RenameProjectItemCommand : IUndoableCommand
    {
        private readonly Action<string> _renomear;
        private readonly string _nomeAnterior;
        private readonly string _nomeNovo;

        private RenameProjectItemCommand(Action<string> renomear, string nomeAnterior, string nomeNovo)
        {
            _renomear = renomear ?? throw new ArgumentNullException(nameof(renomear));
            _nomeAnterior = nomeAnterior ?? string.Empty;
            _nomeNovo = nomeNovo ?? string.Empty;
        }

        public static RenameProjectItemCommand Vista(AraciDocument document, ProjectView vista, string nomeNovo)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(vista);

            return new RenameProjectItemCommand(
                nome => document.RenomearVista(vista, nome),
                vista.Nome,
                nomeNovo);
        }

        public static RenameProjectItemCommand Tabela(AraciDocument document, ProjectTable tabela, string nomeNovo)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tabela);

            return new RenameProjectItemCommand(
                nome => document.RenomearTabela(tabela, nome),
                tabela.Nome,
                nomeNovo);
        }

        public static RenameProjectItemCommand Prancha(AraciDocument document, ProjectSheet prancha, string nomeNovo)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(prancha);

            return new RenameProjectItemCommand(
                nome => document.RenomearPrancha(prancha, nome),
                prancha.Nome,
                nomeNovo);
        }

        public static RenameProjectItemCommand TipoPrancha(AraciDocument document, ProjectSheetType tipo, string nomeNovo)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tipo);

            return new RenameProjectItemCommand(
                nome => document.RenomearTipoPrancha(tipo, nome),
                tipo.Nome,
                nomeNovo);
        }

        public void Execute()
        {
            _renomear(_nomeNovo);
        }

        public void Undo()
        {
            _renomear(_nomeAnterior);
        }

        public void Redo()
        {
            Execute();
        }
    }
}
