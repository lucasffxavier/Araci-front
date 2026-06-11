using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class ExcluirCirculoDoTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public ExcluirCirculoDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Excluir(Guid tipoId, Guid circuloId)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return false;

            ProjectSheetTemplateCircle? circulo = tipo.Circulos.FirstOrDefault(c => c.Id == circuloId);

            if (circulo == null)
                return false;

            _commands.Execute(new RemoveProjectSheetTypeCircleCommand(_document, tipo, circulo));
            return true;
        }
    }
}