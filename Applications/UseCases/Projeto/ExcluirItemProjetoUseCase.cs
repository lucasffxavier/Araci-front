using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class ExcluirItemProjetoUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public ExcluirItemProjetoUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool ExcluirVista(Guid id)
        {
            if (_document.Vistas.Count <= 1)
                return false;

            ProjectView? vista = _document.Vistas.FirstOrDefault(v => v.Id == id);

            if (vista == null)
                return false;

            _commands.Execute(DeleteProjectItemCommand.Vista(_document, vista));
            return true;
        }

        public bool ExcluirTabela(Guid id)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            _commands.Execute(DeleteProjectItemCommand.Tabela(_document, tabela));
            return true;
        }

        public bool ExcluirPrancha(Guid id)
        {
            ProjectSheet? prancha = _document.Pranchas.FirstOrDefault(p => p.Id == id);

            if (prancha == null)
                return false;

            _commands.Execute(DeleteProjectItemCommand.Prancha(_document, prancha));
            return true;
        }

        public bool ExcluirTipoPrancha(Guid id)
        {
            if (_document.TiposPrancha.Count <= 1 || _document.TipoPranchaEstaEmUso(id))
                return false;

            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == id);

            if (tipo == null)
                return false;

            _commands.Execute(DeleteProjectItemCommand.TipoPrancha(_document, tipo));
            return true;
        }
    }
}
