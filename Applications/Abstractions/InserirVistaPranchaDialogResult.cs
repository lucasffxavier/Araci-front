using System;

namespace Araci.Applications.Abstractions
{
    public sealed class InserirVistaPranchaDialogResult
    {
        public InserirVistaPranchaDialogResult(Guid sheetId, Guid viewId)
        {
            SheetId = sheetId;
            ViewId = viewId;
        }

        public Guid SheetId { get; }
        public Guid ViewId { get; }
    }
}