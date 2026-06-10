using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class ExcluirLinhaDoTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public ExcluirLinhaDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Excluir(Guid tipoId, Guid linhaId)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return false;

            ProjectSheetTemplateLine? linha = tipo.Linhas.FirstOrDefault(l => l.Id == linhaId);

            if (linha == null)
                return false;

            _commands.Execute(new RemoveProjectSheetTypeLineCommand(_document, tipo, linha));
            return true;
        }
    }
}