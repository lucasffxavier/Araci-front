using Araci.Services.Simulation;
using Araci.Core.Documents;
using System.Collections.Generic;

namespace Araci.Applications.Abstractions
{
    public interface IUserDialogService
    {
        void ShowInfo(string title, string message);

        void ShowWarning(string title, string message);

        void ShowError(string title, string message);

        ElementosTabelaDialogResult? ShowElementosTabelaDialog(
            IReadOnlyList<ProjectTableElementCategory> categorias,
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados);

        FiltrosTabelaDialogResult? ShowFiltrosTabelaDialog(
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
            IReadOnlyList<ProjectViewDialogOption> vistasDisponiveis,
            Guid? filtroVistaId,
            ProjectTableFilterLogicalMode modo,
            IReadOnlyList<ProjectTableFilterRule> filtros);

        OrdenacaoTabelaDialogResult? ShowOrdenacaoTabelaDialog(
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
            IReadOnlyList<ProjectTableSorting> ordenacoes);

        bool Confirm(string title, string message);

        void Show(SimulationMessage message);
    }
}
