using System;
using Araci.Core.Commands;

namespace Araci.Core.Transactions
{
    public class TransactionScope : IDisposable
    {
        // =========================
        // ESTADO
        // =========================

        private readonly CompositeCommand _composite;

        private bool _finalizado;

        private readonly CommandManager _commandManager;

        // =========================
        // CONSTRUTOR
        // =========================

        public TransactionScope(
            CommandManager commandManager)
        {
            _commandManager = commandManager;
            _composite = new CompositeCommand();
        }

        // =========================
        // ADICIONAR
        // =========================

        public void Add(IUndoableCommand command)
        {
            if (_finalizado)
                throw new InvalidOperationException(
                    "Transaction já finalizada.");

            _composite.Add(command);
        }

        // =========================
        // COMMIT
        // =========================

        public void Commit()
        {
            if (_finalizado)
                return;

            if (!_composite.IsEmpty)
            {
                _commandManager.Execute(_composite);
            }

            _finalizado = true;
        }

        // =========================
        // ROLLBACK (PREPARADO)
        // =========================

        public void Rollback()
        {
            _finalizado = true;
        }

        // =========================
        // DISPOSE
        // =========================

        public void Dispose()
        {
            if (!_finalizado)
            {
                Commit();
            }
        }
    }
}