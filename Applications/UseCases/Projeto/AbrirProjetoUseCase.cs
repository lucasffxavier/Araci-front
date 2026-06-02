using System;
using Araci.Applications.Abstractions;

namespace Araci.Applications.UseCases.Projeto
{
    public class AbrirProjetoUseCase
    {
        private readonly IProjectPersistenceService _projects;

        public AbrirProjetoUseCase(IProjectPersistenceService projects)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        }

        public void ExecutarComDialogo()
        {
            _projects.AbrirComDialogo();
        }

        public void Executar(string path)
        {
            _projects.Abrir(path);
        }
    }
}
