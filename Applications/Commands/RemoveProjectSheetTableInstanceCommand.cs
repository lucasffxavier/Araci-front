using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class RemoveProjectSheetTableInstanceCommand : IUndoableCommand
    {
        private readonly ProjectSheet _sheet;
        private readonly ProjectSheetTableInstance _instance;
        private readonly int _index;

        public RemoveProjectSheetTableInstanceCommand(ProjectSheet sheet, ProjectSheetTableInstance instance)
        {
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _index = sheet.Tabelas.FindIndex(i => i.Id == instance.Id);
        }

        public void Execute()
        {
            Remove();
        }

        public void Undo()
        {
            if (_sheet.Tabelas.Exists(i => i.Id == _instance.Id))
                return;

            int safeIndex = _index < 0 || _index > _sheet.Tabelas.Count
                ? _sheet.Tabelas.Count
                : _index;

            _sheet.Tabelas.Insert(safeIndex, _instance);
        }

        public void Redo()
        {
            Remove();
        }

        private void Remove()
        {
            _sheet.Tabelas.RemoveAll(i => i.Id == _instance.Id);
        }
    }
}