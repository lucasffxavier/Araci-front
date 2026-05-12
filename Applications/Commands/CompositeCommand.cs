using System.Collections.Generic;

namespace Araci.Core.Commands
{
    public class CompositeCommand
        : IUndoableCommand
    {
        // =========================
        // COMMANDS
        // =========================

        private readonly List<IUndoableCommand>
            _commands
                = new();

        // =========================
        // ADICIONAR
        // =========================

        public void Add(
            IUndoableCommand command)
        {
            _commands.Add(command);
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            for (int i = _commands.Count - 1;
                 i >= 0;
                 i--)
            {
                _commands[i].Undo();
            }
        }

        // =========================
        // VAZIO
        // =========================

        public bool IsEmpty =>
            _commands.Count == 0;
    }
}