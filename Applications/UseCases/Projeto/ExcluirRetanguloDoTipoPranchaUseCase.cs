using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class ExcluirRetanguloDoTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public ExcluirRetanguloDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Excluir(Guid tipoId, Guid retanguloId)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return false;

            ProjectSheetTemplateRectangle? retangulo = tipo.Retangulos.FirstOrDefault(r => r.Id == retanguloId);

            if (retangulo == null)
                return false;

            _commands.Execute(new RemoveProjectSheetTypeRectangleCommand(_document, tipo, retangulo));
            return true;
        }
    }
}