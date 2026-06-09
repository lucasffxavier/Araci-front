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

        public MoveProjectSheetTableInstanceCommand(
            ProjectSheetTableInstance instance,
            double oldX,
            double oldY,
            double newX,
            double newY)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _oldX = oldX;
            _oldY = oldY;
            _newX = newX;
            _newY = newY;
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
        }
    }
}
