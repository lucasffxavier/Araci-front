using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class ExcluirTextoDoTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public ExcluirTextoDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Excluir(Guid tipoId, Guid textoId)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return false;

            ProjectSheetTemplateText? texto = tipo.Textos.FirstOrDefault(t => t.Id == textoId);

            if (texto == null)
                return false;

            _commands.Execute(new RemoveProjectSheetTypeTextCommand(_document, tipo, texto));
            return true;
        }
    }
}