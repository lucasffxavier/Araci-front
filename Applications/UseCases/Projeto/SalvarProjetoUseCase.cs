using System;
using Araci.Applications.Abstractions;

namespace Araci.Applications.UseCases.Projeto
{
    public class SalvarProjetoUseCase
    {
        private readonly IProjectPersistenceService _projects;

        public SalvarProjetoUseCase(IProjectPersistenceService projects)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        }

        public void ExecutarComDialogo()
        {
            _projects.SalvarComDialogo();
        }

        public void Executar(string path)
        {
            _projects.Salvar(path);
        }
    }
}
