using Araci.Core.Documents;

namespace Araci.Applications.Abstractions
{
    public sealed class OrdenacaoTabelaDialogResult
    {
        public OrdenacaoTabelaDialogResult(ProjectTableSorting? ordenacao)
        {
            Ordenacao = ordenacao;
        }

        public ProjectTableSorting? Ordenacao { get; }
    }
}
