using System;
using System.Collections.Generic;
using System.Linq;
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
            IReadOnlyList<(ProjectTable Tabela, Guid? FiltroVistaId)> tabelasAfetadas = document.Tabelas
                .Where(t => t.FiltroVistaId == vista.Id)
                .Select(t => (t, t.FiltroVistaId))
                .ToList();

            return new DeleteProjectItemCommand(
                () =>
                {
                    document.RemoverVista(vista);
                    AplicarFiltroVista(tabelasAfetadas, null, document);
                },
                () =>
                {
                    document.RestaurarVista(vista, indice);

                    if (vistaAtivaAnterior.HasValue)
                        document.DefinirVistaAtiva(vistaAtivaAnterior.Value);

                    RestaurarFiltrosVista(tabelasAfetadas, document);
                });
        }

        public static DeleteProjectItemCommand Tabela(AraciDocument document, ProjectTable tabela)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tabela);

            int indice = document.Tabelas.IndexOf(tabela);
            IReadOnlyList<SheetTableInstancesSnapshot> instanciasAfetadas = document.Pranchas
                .Select(prancha => new SheetTableInstancesSnapshot(
                    prancha,
                    prancha.Tabelas
                        .Select((instancia, index) => new { Instancia = instancia, Index = index })
                        .Where(item => item.Instancia.TableId == tabela.Id)
                        .Select(item => new SheetTableInstanceSnapshot(item.Index, item.Instancia.CriarCopia(gerarNovoId: false)))
                        .ToList()))
                .Where(snapshot => snapshot.Instancias.Count > 0)
                .ToList();

            return new DeleteProjectItemCommand(
                () =>
                {
                    document.RemoverTabela(tabela);
                    RemoverInstanciasTabela(instanciasAfetadas, tabela.Id);
                },
                () =>
                {
                    document.RestaurarTabela(tabela, indice);
                    RestaurarInstanciasTabela(instanciasAfetadas);
                });
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

        private static void AplicarFiltroVista(
            IReadOnlyList<(ProjectTable Tabela, Guid? FiltroVistaId)> tabelas,
            Guid? filtroVistaId,
            AraciDocument document)
        {
            foreach ((ProjectTable tabela, _) in tabelas)
            {
                tabela.FiltroVistaId = filtroVistaId;
                document.AtualizarPropriedadesTabela(tabela);
            }
        }

        private static void RestaurarFiltrosVista(
            IReadOnlyList<(ProjectTable Tabela, Guid? FiltroVistaId)> tabelas,
            AraciDocument document)
        {
            foreach ((ProjectTable tabela, Guid? filtroVistaId) in tabelas)
            {
                tabela.FiltroVistaId = filtroVistaId;
                document.AtualizarPropriedadesTabela(tabela);
            }
        }

        private static void RemoverInstanciasTabela(
            IReadOnlyList<SheetTableInstancesSnapshot> snapshots,
            Guid tableId)
        {
            foreach (SheetTableInstancesSnapshot snapshot in snapshots)
                snapshot.Prancha.Tabelas.RemoveAll(i => i.TableId == tableId);
        }

        private static void RestaurarInstanciasTabela(IReadOnlyList<SheetTableInstancesSnapshot> snapshots)
        {
            foreach (SheetTableInstancesSnapshot snapshot in snapshots)
            {
                foreach (SheetTableInstanceSnapshot instancia in snapshot.Instancias.OrderBy(i => i.Index))
                {
                    int indiceSeguro = instancia.Index < 0 || instancia.Index > snapshot.Prancha.Tabelas.Count
                        ? snapshot.Prancha.Tabelas.Count
                        : instancia.Index;

                    snapshot.Prancha.Tabelas.Insert(indiceSeguro, instancia.Instancia.CriarCopia(gerarNovoId: false));
                }
            }
        }

        private sealed class SheetTableInstancesSnapshot
        {
            public SheetTableInstancesSnapshot(
                ProjectSheet prancha,
                IReadOnlyList<SheetTableInstanceSnapshot> instancias)
            {
                Prancha = prancha;
                Instancias = instancias;
            }

            public ProjectSheet Prancha { get; }
            public IReadOnlyList<SheetTableInstanceSnapshot> Instancias { get; }
        }

        private sealed class SheetTableInstanceSnapshot
        {
            public SheetTableInstanceSnapshot(int index, ProjectSheetTableInstance instancia)
            {
                Index = index;
                Instancia = instancia;
            }

            public int Index { get; }
            public ProjectSheetTableInstance Instancia { get; }
        }
    }
}
