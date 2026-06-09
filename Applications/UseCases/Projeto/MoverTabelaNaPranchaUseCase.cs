using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverTabelaNaPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public MoverTabelaNaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Mover(Guid sheetId, Guid instanceId, double novoX, double novoY, Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);
            ProjectSheetTableInstance? instance = sheet?.Tabelas.FirstOrDefault(i => i.Id == instanceId);

            if (sheet == null || instance == null)
                return false;

            if (Math.Abs(instance.X - novoX) < 0.000001 && Math.Abs(instance.Y - novoY) < 0.000001)
                return false;

            _commands.Execute(new MoveProjectSheetTableInstanceCommand(instance, instance.X, instance.Y, novoX, novoY, onChanged));
            return true;
        }
    }
}