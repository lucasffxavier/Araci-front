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
        // TRANSACTION
        // =========================

        private CompositeCommand?
            _transactionAtual;

        // =========================
        // EXECUTE
        // =========================

        public void Execute(
            IUndoableCommand command)
        {
            // =========================
            // TRANSACTION ATIVA
            // =========================

            if (_transactionAtual != null)
            {
                _transactionAtual.Add(command);

                command.Execute();

                return;
            }

            // =========================
            // NORMAL
            // =========================

            command.Execute();

            _undoStack.Push(command);

            _redoStack.Clear();
        }

        // =========================
        // BEGIN TRANSACTION
        // =========================

        public void BeginTransaction()
        {
            if (_transactionAtual != null)
                return;

            _transactionAtual =
                new CompositeCommand();
        }

        // =========================
        // COMMIT
        // =========================

        public void CommitTransaction()
        {
            if (_transactionAtual == null)
                return;

            if (!_transactionAtual.IsEmpty)
            {
                _undoStack.Push(
                    _transactionAtual);

                _redoStack.Clear();
            }

            _transactionAtual = null;
        }

        // =========================
        // ROLLBACK
        // =========================

        public void RollbackTransaction()
        {
            if (_transactionAtual == null)
                return;

            _transactionAtual.Undo();

            _transactionAtual = null;
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
    }
}