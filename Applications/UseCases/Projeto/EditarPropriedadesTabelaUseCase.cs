using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class EditarPropriedadesTabelaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public EditarPropriedadesTabelaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool AlterarDisciplina(Guid id, ProjectViewDiscipline disciplina)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            if (tabela.Disciplina == disciplina)
                return true;

            _commands.Execute(new UpdateProjectTablePropertyCommand<ProjectViewDiscipline>(
                _document,
                tabela,
                (t, valor) => t.Disciplina = valor,
                tabela.Disciplina,
                disciplina));

            return true;
        }
    }
}
