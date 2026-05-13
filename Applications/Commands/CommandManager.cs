using System.Collections.Generic;

namespace Araci.Core.Commands
{
    public class CommandManager
    {
        // =========================
        // HISTÓRICO
        // =========================

        private readonly Stack<IUndoableCommand>
            _undoStack
                = new();

        private readonly Stack<IUndoableCommand>
            _redoStack
                = new();

        // =========================
        // EXECUTE
        // =========================

        public void Execute(
            IUndoableCommand command)
        {
            command.Execute();

            _undoStack.Push(command);

            _redoStack.Clear();
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            if (_undoStack.Count == 0)
                return;

            var command =
                _undoStack.Pop();

            command.Undo();

            _redoStack.Push(command);
        }

        // =========================
        // REDO
        // =========================

        public void Redo()
        {
            if (_redoStack.Count == 0)
                return;

            var command =
                _redoStack.Pop();

            command.Execute();

            _undoStack.Push(command);
        }

        // =========================
        // FLAGS
        // =========================

        public bool CanUndo =>
            _undoStack.Count > 0;

        public bool CanRedo =>
            _redoStack.Count > 0;
    }
}