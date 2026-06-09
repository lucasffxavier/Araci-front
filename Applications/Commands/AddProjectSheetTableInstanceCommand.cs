using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class AddProjectSheetTableInstanceCommand : IUndoableCommand
    {
        private readonly ProjectSheet _sheet;
        private readonly ProjectSheetTableInstance _instance;
        private readonly int _index;
        private readonly Action? _onChanged;

        public AddProjectSheetTableInstanceCommand(ProjectSheet sheet, ProjectSheetTableInstance instance, int? index = null, Action? onChanged = null)
        {
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _index = index ?? sheet.Tabelas.Count;
            _onChanged = onChanged;
        }

        public void Execute()
        {
            Add();
        }

        public void Undo()
        {
            if (_sheet.Tabelas.RemoveAll(i => i.Id == _instance.Id) > 0)
                NotifyChanged();
        }

        public void Redo()
        {
            Add();
        }

        private void Add()
        {
            if (_sheet.Tabelas.Any(i => i.Id == _instance.Id))
                return;

            int safeIndex = _index < 0 || _index > _sheet.Tabelas.Count
                ? _sheet.Tabelas.Count
                : _index;

            _sheet.Tabelas.Insert(safeIndex, _instance);
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            _onChanged?.Invoke();
        }
    }
}