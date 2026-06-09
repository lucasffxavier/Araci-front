using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class RemoveProjectSheetTableInstanceCommand : IUndoableCommand
    {
        private readonly ProjectSheet _sheet;
        private readonly ProjectSheetTableInstance _instance;
        private readonly int _index;
        private readonly Action? _onChanged;

        public RemoveProjectSheetTableInstanceCommand(ProjectSheet sheet, ProjectSheetTableInstance instance, Action? onChanged = null)
        {
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _index = sheet.Tabelas.FindIndex(i => i.Id == instance.Id);
            _onChanged = onChanged;
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
            NotifyChanged();
        }

        public void Redo()
        {
            Remove();
        }

        private void Remove()
        {
            if (_sheet.Tabelas.RemoveAll(i => i.Id == _instance.Id) > 0)
                NotifyChanged();
        }

        private void NotifyChanged()
        {
            _onChanged?.Invoke();
        }
    }
}