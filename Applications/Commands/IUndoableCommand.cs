namespace Araci.Core.Commands
{
    public interface IUndoableCommand
    {
        void Execute();

        void Undo();

        void Redo();
    }
}