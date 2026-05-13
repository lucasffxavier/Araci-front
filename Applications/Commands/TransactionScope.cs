using System;

using Araci.Core.Commands;

namespace Araci.Core.Transactions
{
    public class TransactionScope
        : IDisposable
    {
        private readonly CompositeCommand
            _composite;

        private readonly CommandManager
            _commandManager;

        private bool
            _finalizado;

        public TransactionScope(
            CommandManager commandManager)
        {
            _commandManager =
                commandManager;

            _composite =
                new CompositeCommand();
        }

        public void Add(
            IUndoableCommand command)
        {
            if (_finalizado)
            {
                throw new InvalidOperationException(
                    "Transaction finalizada.");
            }

            _composite.Add(command);
        }

        public void Commit()
        {
            if (_finalizado)
                return;

            if (!_composite.IsEmpty)
            {
                _commandManager.Execute(
                    _composite);
            }

            _finalizado = true;
        }

        public void Rollback()
        {
            _finalizado = true;
        }

        public void Dispose()
        {
            // NÃO COMMITA AUTOMATICAMENTE
            // Segurança transacional
        }
    }
}