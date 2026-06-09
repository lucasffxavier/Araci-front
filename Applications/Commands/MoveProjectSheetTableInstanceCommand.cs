using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class MoveProjectSheetTableInstanceCommand : IUndoableCommand
    {
        private readonly ProjectSheetTableInstance _instance;
        private readonly double _oldX;
        private readonly double _oldY;
        private readonly double _newX;
        private readonly double _newY;
        private readonly Action? _onChanged;

        public MoveProjectSheetTableInstanceCommand(
            ProjectSheetTableInstance instance,
            double oldX,
            double oldY,
            double newX,
            double newY,
            Action? onChanged = null)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _oldX = oldX;
            _oldY = oldY;
            _newX = newX;
            _newY = newY;
            _onChanged = onChanged;
        }

        public void Execute()
        {
            Apply(_newX, _newY);
        }

        public void Undo()
        {
            Apply(_oldX, _oldY);
        }

        public void Redo()
        {
            Apply(_newX, _newY);
        }

        private void Apply(double x, double y)
        {
            _instance.X = x;
            _instance.Y = y;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            _onChanged?.Invoke();
        }
    }
}