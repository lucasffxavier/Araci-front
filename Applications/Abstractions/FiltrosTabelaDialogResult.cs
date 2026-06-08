using System.Collections.Generic;
using Araci.Core.Documents;

namespace Araci.Applications.Abstractions
{
    public sealed class FiltrosTabelaDialogResult
    {
        public FiltrosTabelaDialogResult(
            Guid? filtroVistaId,
            ProjectTableFilterLogicalMode modo,
            IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            FiltroVistaId = filtroVistaId;
            Modo = modo;
            Filtros = filtros;
        }

        public Guid? FiltroVistaId { get; }
        public ProjectTableFilterLogicalMode Modo { get; }
        public IReadOnlyList<ProjectTableFilterRule> Filtros { get; }
    }
}
