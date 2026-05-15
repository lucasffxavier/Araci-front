using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCarga
{
    public class InserirCargaApplication
    {
        // =========================
        // EXECUTAR
        // =========================

        public void Executar()
        {
            CargaViewModel vm =
                ElementoFactory
                    .CriarCargaVM();

            AppServices.Commands.Execute(
                new AddElementoCommand(vm));
        }
    }
}
