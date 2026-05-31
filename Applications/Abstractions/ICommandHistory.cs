using Araci.Core.Commands;
using Araci.Core.Transactions;

namespace Araci.Applications.Abstractions
{
    public interface ICommandHistory
    {
        void Execute(IUndoableCommand command);

        void Undo();

        void Redo();

        void Clear();

        TransactionScope BeginTransaction();
    }
}
