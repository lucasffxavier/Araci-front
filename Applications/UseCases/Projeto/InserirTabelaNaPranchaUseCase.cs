using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class InserirTabelaNaPranchaUseCase
    {
        public const double DefaultX = 40.0;
        public const double DefaultY = 40.0;
        public const double DefaultWidth = 400.0;
        public const double DefaultHeight = 240.0;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public InserirTabelaNaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectSheetTableInstance? Inserir(
            Guid sheetId,
            Guid tableId,
            double? x = null,
            double? y = null,
            double? width = null,
            double? height = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);

            if (sheet == null || !_document.Tabelas.Any(t => t.Id == tableId))
                return null;

            var instance = new ProjectSheetTableInstance
            {
                TableId = tableId,
                X = x ?? DefaultX,
                Y = y ?? DefaultY,
                Width = width ?? DefaultWidth,
                Height = height ?? DefaultHeight
            };

            _commands.Execute(new AddProjectSheetTableInstanceCommand(sheet, instance));
            return instance;
        }
    }
}
