using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class DeleteProjectItemCommand : IUndoableCommand
    {
        private readonly Action _remover;
        private readonly Action _restaurar;

        private DeleteProjectItemCommand(Action remover, Action restaurar)
        {
            _remover = remover ?? throw new ArgumentNullException(nameof(remover));
            _restaurar = restaurar ?? throw new ArgumentNullException(nameof(restaurar));
        }

        public static DeleteProjectItemCommand Vista(AraciDocument document, ProjectView vista)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(vista);

            int indice = document.Vistas.IndexOf(vista);
            Guid? vistaAtivaAnterior = document.VistaAtivaId;

            return new DeleteProjectItemCommand(
                () => document.RemoverVista(vista),
                () =>
                {
                    document.RestaurarVista(vista, indice);

                    if (vistaAtivaAnterior.HasValue)
                        document.DefinirVistaAtiva(vistaAtivaAnterior.Value);
                });
        }

        public static DeleteProjectItemCommand Tabela(AraciDocument document, ProjectTable tabela)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tabela);

            int indice = document.Tabelas.IndexOf(tabela);

            return new DeleteProjectItemCommand(
                () => document.RemoverTabela(tabela),
                () => document.RestaurarTabela(tabela, indice));
        }

        public static DeleteProjectItemCommand Prancha(AraciDocument document, ProjectSheet prancha)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(prancha);

            int indice = document.Pranchas.IndexOf(prancha);

            return new DeleteProjectItemCommand(
                () => document.RemoverPrancha(prancha),
                () => document.RestaurarPrancha(prancha, indice));
        }

        public void Execute()
        {
            _remover();
        }

        public void Undo()
        {
            _restaurar();
        }

        public void Redo()
        {
            Execute();
        }
    }
}
