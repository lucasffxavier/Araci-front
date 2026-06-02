using System;
using Araci.Applications.Abstractions;

namespace Araci.Applications.UseCases.Projeto
{
    public class NovoProjetoUseCase
    {
        private readonly IProjectPersistenceService _projects;

        public NovoProjetoUseCase(IProjectPersistenceService projects)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        }

        public void Executar()
        {
            _projects.Novo();
        }
    }
}
