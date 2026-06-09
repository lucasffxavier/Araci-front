using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class RemoverTabelaDaPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public RemoverTabelaDaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Remover(Guid sheetId, Guid instanceId, Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);
            ProjectSheetTableInstance? instance = sheet?.Tabelas.FirstOrDefault(i => i.Id == instanceId);

            if (sheet == null || instance == null)
                return false;

            _commands.Execute(new RemoveProjectSheetTableInstanceCommand(sheet, instance, onChanged));
            return true;
        }
    }
}