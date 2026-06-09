using System;
using System.Collections.Generic;
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
        public const double DefaultVerticalSpacing = 20.0;

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
            double? height = null,
            Action? onChanged = null)
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

            _commands.Execute(new AddProjectSheetTableInstanceCommand(sheet, instance, onChanged: onChanged));
            return instance;
        }

        public IReadOnlyList<ProjectSheetTableInstance> InserirMultiplas(Guid sheetId, IEnumerable<Guid> tableIds, Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);

            if (sheet == null || tableIds == null)
                return Array.Empty<ProjectSheetTableInstance>();

            List<Guid> validTableIds = tableIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .Where(id => _document.Tabelas.Any(t => t.Id == id))
                .ToList();

            if (validTableIds.Count == 0)
                return Array.Empty<ProjectSheetTableInstance>();

            var composite = new CompositeCommand();
            var instances = new List<ProjectSheetTableInstance>();
            int baseIndex = sheet.Tabelas.Count;

            for (int i = 0; i < validTableIds.Count; i++)
            {
                var instance = new ProjectSheetTableInstance
                {
                    TableId = validTableIds[i],
                    X = DefaultX,
                    Y = DefaultY + i * (DefaultHeight + DefaultVerticalSpacing),
                    Width = DefaultWidth,
                    Height = DefaultHeight
                };

                instances.Add(instance);
                composite.Add(new AddProjectSheetTableInstanceCommand(sheet, instance, baseIndex + i, onChanged));
            }

            _commands.Execute(composite);
            return instances;
        }
    }
}