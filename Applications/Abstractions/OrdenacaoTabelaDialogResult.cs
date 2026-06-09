using Araci.Core.Documents;
using System.Collections.Generic;
using System.Linq;

namespace Araci.Applications.Abstractions
{
    public sealed class OrdenacaoTabelaDialogResult
    {
        public OrdenacaoTabelaDialogResult(IReadOnlyList<ProjectTableSorting> ordenacoes)
        {
            Ordenacoes = ordenacoes.ToList();
        }

        public IReadOnlyList<ProjectTableSorting> Ordenacoes { get; }
    }
}
