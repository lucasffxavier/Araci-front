using System.Collections.Generic;
using Araci.Core.Documents;

namespace Araci.Applications.Abstractions
{
    public sealed class FiltrosTabelaDialogResult
    {
        public FiltrosTabelaDialogResult(
            ProjectTableFilterLogicalMode modo,
            IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            Modo = modo;
            Filtros = filtros;
        }

        public ProjectTableFilterLogicalMode Modo { get; }
        public IReadOnlyList<ProjectTableFilterRule> Filtros { get; }
    }
}
