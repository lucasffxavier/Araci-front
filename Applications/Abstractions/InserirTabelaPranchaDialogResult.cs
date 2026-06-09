using System;
using System.Collections.Generic;
using System.Linq;

namespace Araci.Applications.Abstractions
{
    public sealed class InserirTabelaPranchaDialogResult
    {
        public InserirTabelaPranchaDialogResult(Guid sheetId, IEnumerable<Guid> tableIds)
        {
            SheetId = sheetId;
            TableIds = (tableIds ?? Array.Empty<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
        }

        public Guid SheetId { get; }
        public IReadOnlyList<Guid> TableIds { get; }
    }
}
