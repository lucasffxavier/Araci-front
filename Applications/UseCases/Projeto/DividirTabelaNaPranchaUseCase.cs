using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Applications.Projects.Tables;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class DividirTabelaNaPranchaUseCase
    {
        public const double DefaultSplitSpacing = 24.0;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;
        private readonly ProjectTableDataBuilder _tableDataBuilder = new();

        public DividirTabelaNaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectSheetTableInstance? Dividir(Guid sheetId, Guid instanceId, Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);
            ProjectSheetTableInstance? original = sheet?.Tabelas.FirstOrDefault(i => i.Id == instanceId);

            if (sheet == null || original == null)
                return null;

            if (original.RowStartIndex != 0 || original.RowCount.HasValue)
                return null;

            ProjectTable? table = _document.Tabelas.FirstOrDefault(t => t.Id == original.TableId);

            if (table == null)
                return null;

            ProjectTableDataResult data = _tableDataBuilder.Build(_document, table);
            int totalRows = data.Rows.Count;

            if (data.Columns.Count == 0 || totalRows < 2)
                return null;

            int firstCount = (totalRows + 1) / 2;
            int remainingCount = totalRows - firstCount;

            if (remainingCount <= 0)
                return null;

            var newInstance = new ProjectSheetTableInstance
            {
                TableId = original.TableId,
                X = original.X + original.Width + DefaultSplitSpacing,
                Y = original.Y,
                Width = original.Width,
                Height = original.Height,
                RowStartIndex = firstCount,
                RowCount = remainingCount
            };

            int originalIndex = sheet.Tabelas.FindIndex(i => i.Id == original.Id);
            int insertIndex = originalIndex < 0 ? sheet.Tabelas.Count : originalIndex + 1;

            _commands.Execute(new SplitProjectSheetTableInstanceCommand(
                sheet,
                original,
                newInstance,
                0,
                firstCount,
                insertIndex,
                onChanged));

            return newInstance;
        }
    }
}
