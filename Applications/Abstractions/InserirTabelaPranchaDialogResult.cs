using System;

namespace Araci.Applications.Abstractions
{
    public sealed class InserirTabelaPranchaDialogResult
    {
        public InserirTabelaPranchaDialogResult(Guid sheetId, Guid tableId)
        {
            SheetId = sheetId;
            TableId = tableId;
        }

        public Guid SheetId { get; }
        public Guid TableId { get; }
    }
}
