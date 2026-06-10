using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class InserirLinhaNoTipoPranchaUseCase
    {
        private const double MinLengthSquared = 0.0001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public InserirLinhaNoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectSheetTemplateLine? Inserir(Guid tipoId, double x1, double y1, double x2, double y2)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null || !TemComprimentoValido(x1, y1, x2, y2))
                return null;

            var linha = new ProjectSheetTemplateLine
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            _commands.Execute(new AddProjectSheetTypeLineCommand(_document, tipo, linha));
            return linha;
        }

        private static bool TemComprimentoValido(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return dx * dx + dy * dy >= MinLengthSquared;
        }
    }
}
