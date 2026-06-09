using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class SplitProjectSheetTableInstanceCommand : IUndoableCommand
    {
        private readonly ProjectSheet _sheet;
        private readonly ProjectSheetTableInstance _originalInstance;
        private readonly ProjectSheetTableInstance _newInstance;
        private readonly int _insertIndex;
        private readonly int _oldRowStartIndex;
        private readonly int? _oldRowCount;
        private readonly int _newOriginalRowStartIndex;
        private readonly int? _newOriginalRowCount;
        private readonly Action? _onChanged;

        public SplitProjectSheetTableInstanceCommand(
            ProjectSheet sheet,
            ProjectSheetTableInstance originalInstance,
            ProjectSheetTableInstance newInstance,
            int newOriginalRowStartIndex,
            int? newOriginalRowCount,
            int? insertIndex = null,
            Action? onChanged = null)
        {
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _originalInstance = originalInstance ?? throw new ArgumentNullException(nameof(originalInstance));
            _newInstance = newInstance ?? throw new ArgumentNullException(nameof(newInstance));
            _insertIndex = insertIndex ?? sheet.Tabelas.FindIndex(i => i.Id == originalInstance.Id) + 1;
            _oldRowStartIndex = originalInstance.RowStartIndex;
            _oldRowCount = originalInstance.RowCount;
            _newOriginalRowStartIndex = newOriginalRowStartIndex;
            _newOriginalRowCount = newOriginalRowCount;
            _onChanged = onChanged;
        }

        public void Execute()
        {
            ApplySplit();
        }

        public void Undo()
        {
            _sheet.Tabelas.RemoveAll(i => i.Id == _newInstance.Id);
            _originalInstance.RowStartIndex = _oldRowStartIndex;
            _originalInstance.RowCount = _oldRowCount;
            NotifyChanged();
        }

        public void Redo()
        {
            ApplySplit();
        }

        private void ApplySplit()
        {
            _originalInstance.RowStartIndex = _newOriginalRowStartIndex;
            _originalInstance.RowCount = _newOriginalRowCount;

            if (!_sheet.Tabelas.Any(i => i.Id == _newInstance.Id))
            {
                int safeIndex = _insertIndex < 0 || _insertIndex > _sheet.Tabelas.Count
                    ? _sheet.Tabelas.Count
                    : _insertIndex;

                _sheet.Tabelas.Insert(safeIndex, _newInstance);
            }

            NotifyChanged();
        }

        private void NotifyChanged()
        {
            _onChanged?.Invoke();
        }
    }
}
