using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirGerador
{
    public class InserirGeradorApplication
    {
        // =========================
        // EXECUTAR
        // =========================

        public void Executar()
        {
            GeradorViewModel vm =
                ElementoFactory
                    .CriarGeradorVM();

            AppServices.Commands.Execute(
                new AddElementoCommand(vm));
        }
    }
}
